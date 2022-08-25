# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). This project **does not** adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1.47] - 2022-08-09

### Changed
- Parameterized IAC to provide APIM SKU based on environment
- Server parameters for Postgres in the Postgres ARM templates to increase logging

### Added
- Unauthorized banner to Participant Search, Match Search, Match Detail, and Match List pages. 
- Unauthorized Page for when users don't have a location or a role claim
- Support for tests that render & test CSHTML pages  
- Script that generates a dependency report containing a list of .Net and JS dependencies
- Created Log Analytics workspace for all Diagnostic Settings, updated IAC scripts to configure Settings in various resources

### Fixed
- Applied authentication to Bulk Upload & Upload Status endpoints
- Added required participant_id to the request body in the APIM test script for Match API
- Updated ETL Performance Testing tool to incorporate recent changes for Bulk Upload Payload encryption and new restrictions on allowed values for case_id & participant_id
- Removed ID and Publisher from ETL Status API response
- Reordered Search Reasons for Participant Search Page
- Increased security of Storage Accounts
- Fixed and upgraded Application Insights for Azure Functions to use Log Analytics workspace

## [1.1.1.46] - 2022-08-09

### Changed
- HttpsOnly setting to true for Dashboard and Query Tool app services
- Enabled all states and added MO for tts/test environment

### Added
- Ability to close a match based on either states both marking the match invalid or both providing a Final Disposition & Final Disposition Date
- Warning modal that pops up for Vulnerable individuals when the individual is navigated to from a duplicate participation search, from the match search screen, or when directly navigating to the match detail screen
- Match Detail back button that navigates to appropriate prior screen
- Ability to capture all matches and resolution events in the Metrics database
- Blazor toolip component to the Component library
- Tooltips on the Invalid Match and Vulnerable Individual parts of the Match Detail page
- Optional tableName parameter to functions in db_common bash script for specifying table-level access

## [1.1.1.45] - 2022-07-27

### Changed
- Initial Action Date is now defaulted when Initial Action is chosen on Match Detail page.
- Initial Action section is now disabled on Match Detail page when the match is marked Invalid
- Final Disposition Date is now defaulted in some cases when Final Disposition is chosen on Match Detail page.
- Final Disposition section is now disabled on Match Detail page when Initial Action is not yet chosen.
- When a user who does not have permission to edit a match goes to the Match Detail page, they now see a read-only view.

### Added
- Search Reason added to "Search for SNAP Participants" page
- Initial Action and Initial Action Date validation added to Match Detail page & Match Res API.
- Final Disposition and Final Disposition Date validation added to Match Detail page & Match Res API.
- Duplicate Participant searches are now captured and saved to the Metrics database.

### Fixed
- Removed the use of the Azure CLI to get the Event Hub authorization rule ID.
- Match Resolution API's Add Event endpoint now closes the match when Final Disposition is set on both states.
- Participation Bulk Insert now logs information correctly instead of only to the debugger.
- Removed duplicate code relating to Match Disposition

## [1.1.1.44] - 2022-07-12

### Changed
- Made Participant Id a required field for Duplicate Participation API search requests

### Added
- Vulnerable & Invalid Status component/section to Match Details page
- Initial Action component/section to Match Details page
- Final Disposition component/section to the Match Details page
- Save functionality for Match Detail Resolution fields
- New API endpoint for retrieving Bulk Upload status details by its upload identifier
- Validation for Participant Id and Case Id in Duplicate Participation API
- IAC Environmental support for states.csv file
- Radio, Radio Group, Radio Group Input, and Select Components
- Modal Window Support and Navigation blocking behavior

### Fixed
- Pulled State Metadata SQL data insertion out of Match specific SQL file
- Replaced placeholder email addresses with safe-to-use top level testing domain 

## [1.1.1.43] - 2022-06-28

### Changed
- Added support for capturing participant upload status and start/completion timestamps in the Metrics database (retroactively included in v1.1.1)

### Added
- State API internal documentation
- IAC documentation updates for recently added environment variables, AZ CLI updates, and Sha256Sum dependency
- Role checks for Match Details and Match ID searching.
- IAC support for registering Visual Studio as an authorized client application in Azure
- Display for Match creation details and days since a Match was found/created to the Match Details page
- Framework for Resolution portion of Match Details page

### Fixed
- Moved Match Resolution documenation out of Duplicate Participation API documentation to internal Match Resolution API documentation.
- Cypress tests failures caused by States API

## [1.1.1.42] - 2022-06-15

### Changed
- Improved documentation for Duplicate Participation API
- Moved location & role authorization logic from page level to application
- Updated IAC scripts to support Azure US Gov & Azure CLI 2.37

### Added
- Added column encryption support for sensitive PII values in Postgres (retroactively included in v1.1.1)
- Added internal support and API for State Metadata
- Added basic email notification support/infrastructure
- Added match status, disposition information, state contact information, and participant information to the Match Details page

## [1.1.1] - 2022-06-30

### Changed
- Added support for capturing participant upload status and start/completion timestamps in the Metrics database.

### Added
- Added AES encryption for columns containing sensitive PII values in Postgres

## [1.1.0] - 2022-05-31

### Changed
- Converted Dashboard application to a Blazor web app. Changed upload times to display in user's local time.

### Added
- Added payload encryption for Bulk Uploads.
- Added ability to disable match responses for certain states.
- Added basic role & location based authorization to the Query Tool app.
- Restricted Query Tool search page to only allow searches to be performed by users with state claims. 
- Restricted access to the match listing page to only those users with national office claims.
- Added logging (and PII redaction) for failed participant uploads
- Added storage for state phone and email addresses.

### Fixed
- Removed peer dependencies from package-lock file that was causing npm install to break when building a project.
- Removed end-to-end pa11y tests temporarily until we provide nightly builds with the ability to perform full end-to-end testing.


## [1.0.1.40] - 2022-05-17

### Changed
- Updated Bulk Upload API response to return a unqiue identifier for the upload "upload_id". Updated API documentation to describe this response
- Updated Bulk Upload process to delete state's uploaded file from Azure Storage immediately upon processing completion
- Updated Bulk Upload process to delete all participant records from prior uploads after a successful new upload
- Updated Bulk Upload process to record upload statuses in the database and to record failed uploads
- Refactored Bulk Upload performance testing tools to be more resilient to schema changes
- Updated formatting and parameter descriptions in Duplicate Participation API and Bulk Upload API
- Changed pa11y tests to to run through Cypress rather than CircleCI

### Added
- Added endpoint to Match Resolution API to get all existing matches
- Added page to Query Tool web application to display all existing matches

### Removed
- Removed First & Middle names from sample plaintext-example.csv

### Fixed
- Fixed upload_all_participants endpoint
- Fixed timeout settings in connection strings for Bulkd Upload & Orchestrator Functions
- Fixed search reason validation in Duplicate Participation API responses to provide list of errors for each participant rather general failure response

## [1.0.1.39] - 2022-05-03

### Changed
- Updated Bulk Upload Process to utilize a Storage Trigger rather than an Event Grid Trigger
- Refactored Bulk Upload functionality to move PostgresSQL-specific functionality behind a interface 
- Allow multiple errors for Last Name validation on Query Tool page

### Added
- Added default search_reason value to queries generated on Query Tool page
- Added new UI Component Library tests
- Added status to states' Upload table

### Fixed
- Fixed documentation for upload_all_participants endpoint
- Enabled HSTS
- Include X-Frame-Options header in response from Dashboard app

## [1.0.1.38] - 2022-04-19

### Changed
- Updated codebase to .Net 6
- Introduced Match Record Search page that utilizes Match Resolution API for retrieving match information
- Updated Bulk Upload to utilize Postgres COPY for bulk inserting participant records
- Renamed "Bulk Upload" API to just "Bulk" API
- Changed protect_location to vulnerable_individual in Bulk Upload and Duplicate Participation APIs
- Updated code to use inclusive bounding for both upper & lower bounds when setting db values
- Increased storage (and IOPS) of participant records database
- Adjusted database connection string settings to allow long-running database transactions
### Added
- Added ADR for using Postgres COPY functionality for bulk uploads
- Added ADR for using Blazor Web Assembly
- Added ADR for Blazor Component Library
- New lightweight shared API project for common functionality used by Web apps 
- New shared crytography library that uses Azure Key Vault keys for encrypting/decrypting strings
- New pa11y tests for Query Tool app
- Added search_reason as a required field for Duplicate Participation API requests
- Added vulnerable_individual as an optional parameter to Duplicate Participation API
- Duplicate Participation Api now returns match_url as a link that will contain match details
- Introduced Npgsql.NodaTime plugin to handle setting the proper value for recent_benefit_issuance_dates field
- Added functionality to Bulk Upload to generate & persist unique upload ids
### Fixed
- Fixed documentation for upload_all_participants endpoint
- Fixed configure-defender-and-policy.bash script permissions to allow for execution on Unix based systems.

## [1.0.1.37] - 2022-04-05

### Changed
- Added WAF Rules to handle short file name attacks
- Update Duplicate Participation API to return Match url in match responses
- Replaced recent_benefit_months with recent_benefit_issuance_dates. Accept recent benefit issuance dates in the form of date ranges.
- Updated API documentation for 'recent_benefit_issuance_dates'
- Updated Participant Closing date to be nullable
- Updated blob storage ARM templates to include dependency on parent storage account resource
- Updated APIM match test script to use EB subscription
### Added
- UI Component Library
- Added HSTS Error page and policy for errors
- ADR for using Snake case for JSON and Pascal Case for C#
- New project for common DbFixture to be reused across test projects
### Fixed
- Fixed middleware initialization ordering issue for HSTS
- Fixed search logic to avoid searching within the initiating state's own database for matches.

## [1.0.1.36] - 2022-03-22

### Changed
- Enabled Defender for all resources in IaC scripts
### Added
- Added Match Collaboration page and Error pages.
- Added Match Resolution App and GetMatch endpoint
- Added Bulk Upload Performance Test Runner utility
### Fixed
- Fixed middleware initialization ordering issue for HSTS
- Fixed search logic to avoid searching within the initiating state's own database for matches.

## [1.0.1] - 2022-03-08

### Changed
- Enabled soft blob delete with retention period set to 1 day
- Participant table "benefits_end_date" column replaced with "participant_closing_date"
  - Updated database schema with new name and changed column type
  - Updated IaC code with new column name
  - Updated example csv files with new column name
  - Updated Bulk Upload API to support changed field
  - Updated Duplicate Participation API to support changed field
  - Updated Query Tool with field changes
- Removed "invalid" and "status" columns from "matches" table
- Disabled weak ciphers flagged by security scans
### Added
- Added HSTS support for Dashboard and Query Tool applications
- Incorporated Case Number and Participant ID fields in Query Tool 
  - Added these fields to Duplicate Participation API
  - Documenation updated to reflect these changes
- Visual Studio top-level solution file
- Added Match Resolution Events table (match_res_events)
- Match Resolution Aggregator
- Logging for Event Hub
### Fixed
- Bulk Upload commits entire upload as single transaction rather than one transaction per uploaded participant

## [1.0.0.34] - 2022-02-22

### Added
- Added reference to error 429 in find & duplicate-participation-api
- ADR for using Event Sourcing for Match Resolution
- Allow egress traffic from web apps to OIDC provider

## [1.0.0.33] - 2022-02-08

### Changed
- Updated Engineering Practices doc to specify approach to cross-team review
### Added
- Engineering quick start guide

## [1.0] - 2022-01-25

### Changed
- Documentation to keep up to date with current practices
### Added
- ADR on approach to time storage (i.e., `timestamptz`) in our PostgreSQL databases

## [0.97] - 2022-01-13

### Changed
- Documentation to reference specific Node.js version requirement
### Added
- ADRs for async/await, subsystem testing strategy, Windows app service plan decision
- Log streaming for blob storage
- Automated script for creating APIM subscriptions
- Various documentation clean up
- Custom widdershins templates for documentation generation
### Fixed
- CSV schema to show `lds_hash` as a required field

## [0.96] - 2021-12-28

### Changed
- Storage accounts to use zone redundancy
- Target region for `tts` deployments to support zone redundancy change
- `timestamp` fields to `timestamptz`
- DB connection strings to use `VerifyFull` SSL mode
- Duplicate Participation API documentation to clarify `Content-Length` header
### Added
- Updates to engineering practices doc
- Rate limiting on Query Tool search form
- ADR for per-state databases decision
- Logging for Event Hub
### Fixed
- `DbConnection` handling to allow for connection pooling
- Orchestrator IaC configuration to properly sequence adding network protections

## [0.95] - 2021-12-14

### Added
- Add APIM rate limiting policy Duplicate Participation API
- Stream diagnostic settings for key vault and event grid topics to event hub
### Fixed
- Add Microsoft.Storage service endpoint to function apps subnet to prevent deployment failures
- Fix resource deployment sequencing bug causing deployment failures

## [0.94] - 2021-11-29

### Changed
- Update approach to App ID URI usage for App Service Authentication based on recent Microsoft changes
- Denied default access to Orchestrator function storage account and storage containers in IAC script
- Use cloud-specific AAD endpoint when configuring App Service Auth

## [0.93] - 2021-11-16

### Added
- Network egress protections for Function apps and App Service apps
### Changed
- Updated PPRL guidance to include explicit list of suffixes for removal
- Refactored Dashboard subsystem to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
- Configuration options for containerized IaC environment
- Modified web app dependencies to drop requirement for PhantomJS
### Fixed
- Dashboard configuration to use correct metrics API URI
- Dashboard to display error message when API calls fail

## [0.92] - 2021-11-02

### Added
- `match_id` field in Duplicate Participation match responses
- Explicit session timeout for dashboard and query tool apps
- Front-end dependencies build process for query tool and dashboard apps
### Changed
- Refactored `query-tool` subsystem to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
- Made `uswds` the only production dependency for dashboard and query tool apps
### Fixed
- `test-apim-match-api.bash` to use a secure hash from `example.csv`

## [0.91] - 2021-10-14

### Added
- Match record persistence implementation
### Changed
- Enabled geo-redundancy for `core` and `participants` PostgreSQL databases
- Updated Query Tool to only accept printable characters as input
### Fixed
- Match API participant serialization
- IaC scripts to use updated path for Orchestrator app

## [0.9] - 2021-10-06

### Added
- Foundational components (e.g., database, class structures) for match events
### Changed
- `match` and `etl` subsystems were refactored to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md).
- Enhanced normalization library and applied to inputs in the query tool
- Minor enhancements to `create-apim.bash`
### Removed
- References to the previously planned plain text PII matching endpoint

## [0.8] - 2021-09-21

### Added
- `InitiatingState` header to internal request from APIM to Orchestrator API
- Participants library/subsystem to generalize code across from ETL and Match subsystems
- Normalization code to generate the secure hash digest (de-identified PII)
### Changed
- Metrics subsystem was refactored to reflect [ADR on internal software architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
### Fixed
- Query tool match functionality using the new normalization code and PPRL API
- `authorize-cli.bash` and `test-metricsapi.bash` to work in `AzureCloud` and `AzureUSGovernment`

## [0.7] - 2021-09-08

### Added
- New Privacy-Preserving Record Linkage (PPRL) documentation
- Custom authorization error display and sign-out pages for web apps
### Changed
- Numerous style/layout changes for the dashboard
- Duplicate participation API for PPRL approach
  - base URL is now `/match/v2`
  - `query` renamed to `find_matches` which takes de-identified PII
  - `participant_id` and `case_id` is now required in match responses
- Bulk upload API for PPRL approach
  - base URL is now `/bulk/v2`
  - `first`, `middle`, `last`, `dob`, and `ssn` columns in CSV replaced with `lds_hash`
  - `participant_id` and `case_id` is now required in CSV
### Removed
- `state_abbr` property in duplicate participation API
- Internal per-state Function Apps for duplicate participation API
### Fixed
- Log categories used by App Service resources
### Broke
- Query tool match functionality (temporarily have no support for plain text match queries)

## [0.6] - 2021-08-23

### Added
- OIDC claim-based policy enforcement to query tool and dashboard
### Changed
- Numerous style/layout changes for the query tool
- Azure B2C IDP docs to include notes on updating user claims
### Removed
- `exceptions` field from bulk upload format and APIs
### Fixed
- Front Door and Easy Auth now work together in the query tool and dashboard

## [0.5] - 2021-08-10
### Added
- OpenID Connect (OIDC) authentication to dashboard and query tool
- managed identity to metrics Function Apps and database access
- IaC for streaming logs to an external SIEM via Event Hub
- system account and initiating user to audit logs for API calls
- Defender to all storage accounts in subscription
- CIS benchmark to Policy
- top-level build/test script
### Changed
- duplicate participation API to allow an entire household to be queried for
- App Service instances to use Windows under-the-hood
- query tool to remove lookup API feature and accommodate query API changes
- Front Door to use a designated public file in dashboard and query tool for health check
- duplicate participation Function Apps so they do not hibernate
- Orchestrator Function App so that network egress is through a VNet
### Removed
- Lookup API call; it's been obsoleted by PPRL model
- `METRICS_RESOURCE_GROUP`; folded resources into `RESOURCE_GROUP`
### Fixed
- `update-packages.bash --highest-major`
- Key Vault-related IaC so as to be compatible in either `AzureCloud` or `AzureUSGovernment`

## [0.4] - 2021-06-15
### Added
- `benefits_end_month`, `protect_location`, and `recent_benefit_months` to query response.
- `protect_location` and `recent_benefit_months` to CSV.
- `case_id`, `participant_id` to query tool.
- logging to indicate identity of Function App callers.
- log streaming to an Event Hub for remaining Azure resources.
- documentation for creating an Azure AD B2C OIDC identity provider.
- OIDC support for dashboard and query tool via Easy Auth.
- updated high-level architecture diagram.
### Changed
- `dob` field in CSV to be ISO 8601 formatted.
- CSV backwards compatibility: columns, not just field values, are optional when fields are not required.
### Deprecated
- MM/DD/YYYY format for `dob` field in CSV. Will continue to be accepted along with ISO 8601 format.
### Fixed
- `build.bash deploy` for dashboard and query tool.

## [0.3] - 2021-06-01
### Added
- `case_id`, `participant_id`, and `benefits_end_month` fields to CSV.
- `case_id`, `participant_id`, and `state` properties to query response.
- initial log streaming to an Event Hub for Azure resources.
### Changed
- the query tool so as to display the state abbreviation as "State".
### Deprecated
- `state_abbr` property in query response. It has been replaced by `state`.
### Removed
- `state_name` property from the query response.

## [0.2] - 2021-05-18
### Added
- CUI banner to query tool.
- Improved tooling for automated builds, tests, and deploys.
- Shellcheck to the Continuous Integration (CI) process.
### Changed
- Date of Birth (DoB) display format in query tool, just show the month/day/year.

## [0.1] - 2021-05-04
### Added
- Initial APIs for use by group 1A state integrators.

[1.1.1.44]: https://github.com/18F/piipan/releases/tag/v1.1.1.44
[1.1.1.43]: https://github.com/18F/piipan/releases/tag/v1.1.1.43
[1.1.1.42]: https://github.com/18F/piipan/releases/tag/v1.1.0.42
[1.1.1]: https://github.com/18F/piipan/releases/tag/v1.1.1
[1.1.0]: https://github.com/18F/piipan/releases/tag/v1.1.0
[1.0.1.40]: https://github.com/18F/piipan/releases/tag/v1.0.1.40
[1.0.1.39]: https://github.com/18F/piipan/releases/tag/v1.0.1.39
[1.0.1.38]: https://github.com/18F/piipan/releases/tag/v1.0.1.38
[1.0.1.37]: https://github.com/18F/piipan/releases/tag/v1.0.1.37
[1.0.1.36]: https://github.com/18F/piipan/releases/tag/v1.0.1.36
[1.0.1]: https://github.com/18F/piipan/releases/tag/v1.0.1
[1.0.0.34]: https://github.com/18F/piipan/releases/tag/v1.0.0.34
[1.0.0.33]: https://github.com/18F/piipan/releases/tag/v1.0.0.33
[1.0]: https://github.com/18F/piipan/releases/tag/v1.0
[0.97]: https://github.com/18F/piipan/releases/tag/v0.97
[0.96]: https://github.com/18F/piipan/releases/tag/v0.96
[0.95]: https://github.com/18F/piipan/releases/tag/v0.95
[0.94]: https://github.com/18F/piipan/releases/tag/v0.94
[0.93]: https://github.com/18F/piipan/releases/tag/v0.93
[0.92]: https://github.com/18F/piipan/releases/tag/v0.92
[0.91]: https://github.com/18F/piipan/releases/tag/v0.91
[0.9]: https://github.com/18F/piipan/releases/tag/v0.9
[0.8]: https://github.com/18F/piipan/releases/tag/v0.8
[0.7]: https://github.com/18F/piipan/releases/tag/v0.7
[0.6]: https://github.com/18F/piipan/releases/tag/v0.6
[0.5]: https://github.com/18F/piipan/releases/tag/v0.5
[0.4]: https://github.com/18F/piipan/releases/tag/v0.4
[0.3]: https://github.com/18F/piipan/releases/tag/v0.3
[0.2]: https://github.com/18F/piipan/releases/tag/v0.2
[0.1]: https://github.com/18F/piipan/releases/tag/v0.1
