# 33. Use Liquibase for Db Migrations

Date: 2022-04-28

## Status
 
Proposed
 
## Context

For database changes during early development, we have taken the approach of deleting and recreating the database(s). Without any clients or real data, this simple approach requires little effort and works fine. However, now that states are being onboarded for the MVP we need a strategy for database migrations that avoids deleting existing data.

There are several tools available that offer support for database migrations & source control. Liquibase, Redgate, and Flyway are three options we considered.

## Decision

We will use Liquibase for running migration scripts and GitHub for source control of these scripts. 

We quickly eliminated Redgate as an option because it only offers support for SQL Server and Oracle. 

Flyway and Liquibase both offer migrations-based approaches to database changes. Both support SQL-base migrations. They both run from command line. At update time, they both check if changes have already been deployed. 

The reason we chose Liquibase 
* Support of both relational and NoSQL database types.
* Can compare the state of two databases
* Allows rollbacks to undo changes
* More flexible set of options for defining database changes/migrations
* The ability to run dry-runs of migrations

### Workflow

* There will be a master changelog file at the top level or root of each Database's source control folder. Under each folder will be a series of quarterly folders each containing sql migrations created during that period. Each migration is a .sql file(s) and each sibling db-changelog.xml file will reference all of the scripts in that quarterly folder. The reason for the quarterly folders is to prevent the master-changelog.xml file from growing too large. It provides a means of breaking it up into a smaller series of dependent changelog files.
* Scripts should be written with the goal of Evolutionary Database Design in mind
* Scripts (and rollback scripts) should be idempotent if possible. This isn't always possible or in some cases not practical to go to the effort. But we should strive to.
* Script file names should be of the format "mm_dd_{Script description}.sql". Naming should include a good description
* The author should be specified. It should be the author's usda email address.
* Scripts should be saved as a sql file in their appropriate Git folder.
* The Liquibase changelog file should be updated with a changeset reference to the new script
* Changeset Ids should be the same as the sql file name (without the .sql suffix). Liquibase advises that using the file name itself is more unique than a date based naming convention.
* All Changesets should include a comment tag that contains the JIRA ticket number. Just the numeric value ONLY e.g. for Nac-123 you would just put <comment>123</comment>
* Developer commits changes and pushes to Git. Prior to check-in each developer should run both their migration and rollback script through liquibase to verify they are valid, run successfully, and are idempotent. 
* CICD runs migration against DEV databases upon successful PR merge.


## Consequences

* Developers need to install liquibase.
* Documentation needs to be provided for writing migration scripts and for using liquibase to run them.
* DevOps procedures need to be created to run migration scripts.


## Resources
* [Liquibase](https://www.liquibase.org/)
* [Flyway](https://flywaydb.org/)
* [Redgate SQL Source Control](https://www.red-gate.com/products/sql-development/sql-source-control/)
* [Evolutionary Database Design](https://martinfowler.com/bliki/ParallelChange.html)