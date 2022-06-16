# 33. Use Liquibase for Db Migrations

Date: 2022-04-28

## Status
 
Proposed
 
## Context

For database changes during early development, we have taken the approach of deleting and recreating the database(s). Without any clients or real data, this simple approach requires little effort and works fine. However, now that states are being onboarded for the MVP we need a strategy for database migrations that avoids deleting existing data.

There are several tools available that offer support for database migrations & source control. Liquibase, Redgate, DbMaestro, and Flyway are four options we considered.

## Decision

We will use Liquibase for running migration scripts and GitHub for source control of these scripts. 

DbMaestro & Redgate's Source Control and Compare tools are both expensive. Redgate only offers support for SQL Server and Oracle. DbMaestro is not SDK friendly and is more UI driven. They both provide state-based database management which is effectively compare and sync tooling. This is appealing in that it generates upgrade scripts for you, but the problem is that the change process is not repeatable. Another problem with state-based management is that the tooling will push changes forward that may have not been reviewed (e.g. someone has made a manual change to the model database rather than through source control).

Flyway and Liquibase both offer migrations-based approaches to database changes. Both support SQL-base migrations and both run from command line. At update time, they both check if changes have already been deployed. This approach provides a repeatable process at the cost of having to write scripts manually. We see that overhead as beneficial rather than prohibitive. Developers are already writing scripts or making database changes manually. Their changes are then incorporated into IaC scripts and reviewed prior to merging into Git. With this approach, they'd follow a similar process, writing change scripts and submit those scripts for code review prior to merging.

The reason we chose Liquibase over Flyway
* Support of both relational and NoSQL database types.
* Can compare the state of two databases
* Allows rollbacks to undo changes
* More flexible set of options for defining database changes/migrations
* The ability to perform dry-runs of migrations

### Workflow

* There will be a master changelog file at the top level or root of each Database's source control folder. Under each folder will be a series of quarterly folders each containing sql migrations created during that period. Each migration is a .sql file(s) and each sibling db-changelog.xml file will reference all of the scripts in that quarterly folder. The reason for the quarterly folders is to prevent the master-changelog.xml file from growing too large. It provides a means of breaking it up into a smaller series of dependent changelog files.

1. Write sql migration script(s) 
    1. Written with the goal of Evolutionary Database Design in mind
    1. Should be idempotent if possible. This isn't always possible or in some cases not practical. But we should strive for idempotency.
1. Name the file following a convention/format "mm_dd_{Script description}.sql". Naming should include a good description
1. Specify the author. It should be the author's usda email address.
1. Save the script as a sql file in the appropriate Git folder.
1. Update the Liquibase changelog file swith a changeset reference to the new script
1. Make the changeset Ids should be the same as the sql file name (without the .sql suffix). Liquibase advises that using the file name itself is more unique than a date based naming convention.
1. Include a comment tag in the changeset that contains the JIRA ticket number. Just the numeric value ONLY e.g. for Nac-123 you would just put <comment>123</comment>
1. Commit script and changelog updates to Git. Prior to check-in each developer should run both their migration and rollback script through liquibase to verify they are valid, run successfully, and are idempotent. 
1. CICD runs migration against DEV databases upon successful PR merge.


### Example Commands 

* Run an update for Participants Database
1. Navigate to dev/piipan/iac/participants
1. Run -> liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{your host}.postgres.database.azure.com:5432/{your database} --headless=true update

* Rollback an update for Participants Database
1. Navigate to dev/piipan/iac/participants
1. Run -> liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{your host}.postgres.database.azure.com:5432/{your database} --headless=true rollback-count-1

## Consequences

* Developers need to install liquibase.
* Documentation needs to be provided for writing migration scripts and for using liquibase to run them.
* DevOps procedures need to be created to run migration scripts.


## Resources
* [Liquibase](https://www.liquibase.org/)
* [Flyway](https://flywaydb.org/)
* [Redgate SQL Source Control](https://www.red-gate.com/products/sql-development/sql-source-control/)
* [DbMaestro](https://www.dbmaestro.com/)
* [Evolutionary Database Design](https://martinfowler.com/bliki/ParallelChange.html)