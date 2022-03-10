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

## Decision

Because of the performance test results, in addition to the other concerns listed above, we plan to enhance the performance of the match operation using Azure Event Hub as the backend pipeline for the bulk upload API and stream events to multiple Azure function instances (consumers). By incorporating Azure Event Hubs into the bulk upload process, we can stream participants from an upload into the system. 

Azure Event Hubs is a fully managed, real-time data ingestion service that’s simple and scalable. It can be used to stream millions of events per second and build various data pipelines, much like Kafka but without having to manage clusters. It provides real-time data ingestion and microbatching within streams.

We will modify the existing Orchestrator Azure Function. Currently, it is responsible for processing all participants in an upload and committing them to the database. It will be refactored to instead, process all participants, and generate events to a "Participants" Event Hub with a payload containing the participant (or a batch of participants). A new Participant Upload Azure Function (with an EventHub Trigger) will be created that consumes events from the "Participants" Hub and commits the participants (included in the event payload) to the database.

#### Benefits of this approach:

By using Event Hubs, Azure function instances consuming upload requests can operate in parallel, effectively breaking down uploads into chunks, and greatly improving the speed of uploads. This approach separates read/write operations, decoupling the database actions from the upload itself. It also will allow us to build future pipelines off these data streams. For example, we would be positioned to add a pipeline for asynchronously performing match analysis on incoming records. It will make it easier to identify and report on individual record errors. 

#### Drawbacks of this approach:

Even using Event Hubs, fine tuning will be necessary to optimize performance. On the one hand, committing participants one at a time results in a quick Azure Function response, but could cause the database to be a bottleneck as it re-indexes more frequently. On the other hand, if we batch participants, larger batch sizes may result in longer Azure Function response times. We will parameterize the batch size so that we can test different sizes and identify a batch size that results in optimal performance. Depending on 

## Consequences

- We will be able to stream participants from an upload into the database in real-time at a much faster rate.
- With this approach one bad record will not ruin an entire upload.
- We will position ourselves for improved future error reporting.
- We will position ourselves to provide better progress and results notifications to users.
- We will position ourselves to add new data pipelines to process upload data that operate asynchronously and do not affect the upload speed itself.

## Resources

- [Azure Event Hubs documentation](https://docs.microsoft.com/en-us/azure/event-hubs/)
- [Azure Event Hub Partition Balancing](https://docs.microsoft.com/en-us/azure/event-hubs/event-processor-balance-partition-load)
- [Azure Function Event Hub Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger)
- [Azure Function Retry](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-error-pages?tabs=csharp)

