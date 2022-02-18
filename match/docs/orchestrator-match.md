# Orchestrator matching API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial API for matching PII data across all participating states.

The orchestrator matching API is implemented in the `Piipan.Match.Orchestrator` project and deployed to an Azure Function App.

To query the API:
1. A JSON `POST` request that conforms to the [OpenApi spec](openapi.md) is sent to the orchestrator API endpoint.
1. The `POST` event triggers a function named `Find` in the orchestrator Function App.
1. The orchestrator Function App looks for matches across each per-state participant database

## Environment variables

The following environment variables are required by the orchestrator and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `DatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=DatabaseConnectionString) — Additionally, `Database` is set to the placeholder value `{database}`. The relevant per-state database name is inserted at run-time as needed. |
| `States` | [details](../../docs/iac.md#\:\~\:text=States) |

## Local development

Local development is currently limited as a result of using a managed identity to connect to the state databases. The Instance Metadata Service used by managed identities to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.

The app will still build and run locally. However, any valid request sent to the local endpoint will result in an exception when the app attempts to retrieve an access token for the managed identity. Invalid requests (e.g., malformed or missing data in the request body) will return proper error responses.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Unit / integration tests

Unit tests are contained within `Piipan.Match.Orchestrator.Tests` and integration tests within `Piipan.Match.Orchestrator.IntegrationTests`.

To run unit tests:

```
cd match/tests/Piipan.Match.Orchestrator.Tests
dotnet test
```

Integration tests are run by connecting to a PostgreSQL Docker container. With Docker installed on your machine, run the integration tests using Docker Compose:

```
cd match/tests/
docker-compose run --rm app dotnet test /code/match/tests/Piipan.Match.Orchestrator.IntegrationTests/Piipan.Match.Orchestrator.IntegrationTests.csproj
```

## App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Remote testing

To test the orchestrator remotely:
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `OrchestratorApi.Query` role for the remote orchestrator Function App and authorize the Azure CLI.
1. Retrieve a token for your user using the Azure CLI: `az account get-access-token --resource <orchestrator application ID URI>`.
1. Send a request to the remote endpoint—perhaps using a tool like Postman or `curl`—and include the access token in the Authorization header: `Authorization: Bearer {token}`.
