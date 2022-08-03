# Bulk API

## Summary

An external-facing API for uploading [bulk participant records](bulk-import.md) and retrieving the status of past uploads.

Implemented as one API for each participating state, all managed in a single Azure API Management (APIM) instance. The APIM instance is shared with the [duplicate participation API](../../match/duplicate-participation-api.md).

## Endpoints

The API exposes two endpoints:

| Endpoint | Backend |
|---|---|
| `/upload_all_participants/{filename}` | [`Put Blob` operation](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob) |
| `/uploads/{upload_id}` | Per-state ETL `GetUploadStatusById` function |

The base URL for the endpoint is configured per-state according to the following format:

```https://<apim-base-uri>/bulk/<state>/<version>/```

For example, the endpoint for uploading `example.csv` to state EA in the `tts/dev` environment might be:

```https://tts-apim-duppartapi-dev.azure-api.net/bulk/ea/v2/upload_all_participants/example.csv```

## API Management implementation

The [IaC](../../iac/arm-templates/apim.json) creates _N_ per-state APIs within the APIM instance. The API resources are configured to require an active state-specific subscription.

APIM policies are used to handle authentication with backend resources. Additionally, the `upload_all_participants` endpoint uses policies to modify request and response data:
- Adds the required `Date`, `x-ms-version`, and `x-ms-blob-type` request headers and sets them to appropriate values.
- Modifies the response body to include an `upload_id` property set to the uploaded file's [ETag](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob#response-headers).

## Calling the API

To call an endpoint:

1. [Obtain an API key](../../match/docs/duplicate-participation-api.md#managing-api-keys)
1. Send a [valid request](openapi/openapi.yaml), passing the API key in the `Ocp-Apim-Subscription-Key` header
