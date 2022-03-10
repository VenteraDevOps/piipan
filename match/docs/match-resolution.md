# Match Resolution API


## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)


## Summary

An initial API for resolving matches between states. This API is currently only used by the Query and Collaboration Tool, but is built with an eye toward future state integration.

The Match Resolution API is implemented in the `Piipan.Match.Func.ResolutionApi` project and deployed to an Azure Function App.

OpenApi schema is defined [here](./openapi/resolution/index.yaml).


## Environment variables

The following environment variables are required by the API and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `CollaborationDatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=CollaborationDatabaseConnectionString) — Additionally, `Database` is set to the placeholder value `{database}`. The relevant per-state database name is inserted at run-time as needed. |
| `States` | [details](../../docs/iac.md#\:\~\:text=States) |


## Local development

Forthcoming


## Unit / integration tests

Forthcoming


## App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

