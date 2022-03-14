# 28. Event Streaming for bulk uploads & matching

Date: 2022-03-10

## Status

Proposed

## Context

Initially for bulk upload, we took the simplest approach of committing all participants in an upload within a single database transaction. This works fine for small uploads but we anticipated this may not be the appropriate approach for a production-ready system. The expected downsides to this approach are 1) Performance issues. What is the write performance of a transaction containing millions of records? What is the affect on record retrieval when a million or more records are being committed?  2) Any single bad record/participant would cause an entire state upload (likely millions of participants) to fail without clear indication as to the bad record 3) Difficulty communicating upload progress to end-users.

### Testing
To gauge the performance, we ran a series of simple tests. We ran uploads with 100k fake participants into an empty database. Because of the distributed nature of our architecture, we did not collect exact timings. We periodically ran a query to get the count of participants in Postgres. For initial tests, we discovered a bug in the system, lacking a transaction around entire upload. Participants were committed individually. As a result, we ran a second series of tests once a fix was in-place to wrap entire uploads within a transaction.

#### Observations (with individual record commits):
- After polling Postgres for 10 minutes, all participants were not yet in the database although the count was steadily increasing. 
- Multiple records were created in the Uploads table in Postgres. Only 1 should have been created.
- Upon looking at Azure Metrics (specifically the Event Grid System Topic for uploads), we noticed that a) there were numerous upload events b) the upload events kept failing. The failures led Azure Event Grid to automatically retry the uploads following an [exponential backoff retry pattern](https://docs.microsoft.com/en-us/azure/event-grid/delivery-and-retry).
- Azure Metrics for the Etl Azure Function showed corresponding Http Server errors. It also indicated that response times were consistently 230ish seconds (i.e. the maximum timeout setting for Azure Functions).
- Because Event Grid continued to retry the upload, duplicate participants were added. For the 100k test I ran, over a million records were created in the database.

#### Observations (with transaction around entire upload):
- Database records were never created in the database, in neither the Uploads table nor the Participants table.
- Upon looking at Azure Metrics (specifically the Event Grid System Topic for uploads), we noticed that a) there were numerous upload events b) the upload events kept failing. The failures led Azure Event Grid to enter into an exponential backoff retry.

### Options For Addressing Performance Issues
Because of the performance test results, in addition to the other concerns listed above, we plan to enhance the performance of the bulk upload process. We considered a few options for improving performance- Azure Service Bus, Azure Storage Queue, Durable Functions, Azure Batch, & Azure Event Hub. 

As noted previously in ADR 14, while Azure Batch can provide a large amount of compute power on an ad-hoc or scheduled basis, it has the undesired qualities of using resources like VMs and self-contained executable applications which would break out of our PaaS-exclusive philosophy. 

Azure Storage Queues don't support long-polling, parallel long-running streams, at-least-once delivery guartanees, or provide transactional support 

Azure Service Bus messages are pulled out by a receiver & cannot be processed again or by multiple receivers. That would be ok for the main Bulk Upload use-case as it only concerns one consumer group and we don't want competing consumer instances. But looking towards the future, this approach could be limiting when you consider incorporating multiple data analysis streams, processing in parallel. ASB does not support this as a default configurationa and would require a more complex routing topology (e.g. topics that fan-out to separate consumer group topics).

We've considered Durable Functions previously and they would certainly help process uploads more quickly. The fan out / fan in paradigm where an orchestrator stages data and parcels out comparison tasks fits both the current use case and potential expansion to a PPSI (Privacy-Preserving Set Intersection) approach. The downside to an orchestrator messaging paradigm such as this, is the the potential tight coupling between the Azure Orchestrator Function and the Activity Functions, as well as the ability to add new data analysis streams without modifying the Orchestrator Function. 

Azure Event Hubs is a fully managed, real-time data ingestion service that’s simple and scalable. It can be used to stream millions of events per second and build various data pipelines, much like Kafka but without having to manage clusters. It provides real-time data ingestion and microbatching within streams. The downside to Event Hubs is the additional complexity and the importance of setting up a good partitioning strategy. A bad partitioning strategy could result in all events/messages being funneled to a single consumer, overloading it, while leaving the other consumers idle.

## Decision

We plan to enhance the performance of the bulk upload process using Azure Event Hub as the backend pipeline for the bulk upload API and stream events to multiple Azure function instances (consumers). By incorporating Azure Event Hubs into the bulk upload process, we can stream participants from an upload into the system. 

We will modify our existing Orchestrator Azure Function. Currently, it is responsible for processing all participants in an upload and committing them to the database. It will be refactored to instead, to process all participants, and generate events to a "Participants" Event Hub with a payload containing the participant (or a batch of participants). A new Participant Upload Azure Function (with an EventHub Trigger) will be created that consumes events from the "Participants" Hub and commits the participants (included in the event payload) to the database. By splitting the responsibility of this function, it will significantly reduce the response time being provided to Event Grid.

Azure Event Hubs provide a minimum 1 day retention period and a maximum 90 day period (if using Premium or Dedicated plans). This retention period setting is configurable. We don't anticipate needing a retention period this long. We have requirements not to retain data so it's in our best interest to keep this period as short as possible.

Azure Event Hubs provides secure transit of data between senders and receivers via two layers of encryption. Because this data is transmitted within Azure's infrastructure layer (i.e. within Azure private networks and not traveling outside across the internet) the data is encrypted both with TLS 1.2 as well as a data-link encryption method provided by Microsoft.

A few planned implementation details based on our experimentation with Event Hubs
1) We will set up Hubs with more partitions than anticipated consumers. This allows for easier, non-disruptive rebalancing. e.g. 20 partitions and 4 consumers, each consuming 5 partitions. If  scaling is necessary, another consumer is added and the partition assignments are rebalanced -> 5 consumers each consuming 4 partitions OR 10 consumers each consuming 2 partitions. We will run load/performance tests to identify optimum configurations.
2) As we currently don't anticipate any necessary specific partitioning strategy, we will use the [Round Robin](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features#mapping-of-events-to-partitions) strategy providing by Azure Event Hubs by default. i.e. we won't bother specifying a ParitionKey in the code. By doing this events will be load balanced across all consumers.
3) Use [Event Hub Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/event-driven-scaling#event-hubs-trigger) for our Azure functions processing events to take advantage of automatic scaling, load balancing, and partition rebalacing. 

#### Benefits of this approach:

By using Event Hubs, Azure function instances consuming upload requests can operate in parallel, effectively breaking down uploads into chunks, and greatly improving the speed of uploads. This approach separates read/write operations, decoupling the database actions from the upload itself. It also will allow us to build future pipelines off these data streams. For example, we would be positioned to add a pipeline for asynchronously performing match analysis on incoming records. It will make it easier to identify and report on individual record errors. 

#### Drawbacks of this approach:

Even using Event Hubs, fine tuning will be necessary to optimize performance. On the one hand, committing participants one at a time results in a quick Azure Function response, but could cause the database to be a bottleneck as it re-indexes more frequently. On the other hand, if we batch participants, larger batch sizes may result in longer Azure Function response times. We will parameterize the batch size so that we can test different sizes and identify a batch size that results in optimal performance.  

## Consequences

- We will be able to stream participants from an upload into the database in real-time at a much faster rate.
- With this approach one bad record will not ruin an entire upload.
- We will position ourselves for improved future error reporting.
- We will position ourselves to provide better progress and results notifications to users.
- We will position ourselves to add new data pipelines to process upload data that operate asynchronously and do not affect the upload speed itself.

While providing significant performance gains, decoupling the csv parsing from Postgres record persistence presents a couple challenges regardless of the approaches described above. Participants committed in batches (or even record by record) mean partial uploads will exist. We currently only perform matches against the most recent uploads and we clean-up/remove the data from old uploads. So we need to consider 

### 1. Progress monitoring and how to distinguish between successful uploads vs partial/failed uploads?
As a means of monitoring progress, we could track upload participant count as part of upload record. We could record the number of participants belonging to the upload when we create the upload record. Then we could check/monitor progress by issue a count-distinct query against participants. This approach allows us to calculate how many participants have been uploaded at any given time. Duplicates with the csv could be problematic. If we find it's typical that they exist, we may need to modify the query not to be distinct.

### 2. How to perform matches when a new participant failed to be recorded to the database (but an old record of this participant exists)  
Similar to current functionality, the easiest approach would be to only execute matches against the most recent successful upload. We have the option to try to be smarter about this in the future.

### 3. How to clean up old uploads if a new upload only partially succeeded? 
For clean-up, we could only schedule clean-ups after a successful upload succeed (i.e. don't clean-up older uploads if new upload only partially succeeded). We may also alter the clean-up job to remove previous partial/failed uploads (after some set/configured time). Again, we have the option to try and be smarter about this in the future. 

Note* we will need to test the parsing speed of the Orchestrator function. With this refactoring, it will be doing significantly less work but if it still exceeds the Event Grid threshold of 30s responses (https://docs.microsoft.com/en-us/azure/event-grid/delivery-and-retry) an additional change may be necessary. If millions of records require more than 30s to parse out, we may still need to explore ways respond to the Event Grid trigger more quickly while triggering the longer running task asynchronously.

## Resources

- [Azure Event Hubs documentation](https://docs.microsoft.com/en-us/azure/event-hubs/)
- [Azure Event Hub Partition Balancing](https://docs.microsoft.com/en-us/azure/event-hubs/event-processor-balance-partition-load)
- [Azure Function Event Hub Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger)
- [Azure Function Retry](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-error-pages?tabs=csharp)
- [Azure Event Hub Partition Keys](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features#mapping-of-events-to-partitions)
- [Azure Event Hub Dynamically Add Partitions](https://docs.microsoft.com/en-us/azure/event-hubs/dynamically-add-partitions)
- [Azure Event Hub Retention Periods](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features#event-retention)
- [Azure Function Timeout Duration](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#timeout)
- [Azure Service Bus vs Storage Queue](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted) 
- [Azure Encryption of Data in transit](https://docs.microsoft.com/en-us/azure/security/fundamentals/double-encryption#data-in-transit)