
<h1 id="bulk-api">Bulk API v2.0.0</h1>

The API for performing bulk uploads
<h2>File Preparation</h2>
This API requires a file to be submitted with the appropriate schema. The details of that file's schema can be found at <a href='../../../../etl/docs/csv/import-schema.json'>File schema</a>

Base URLs:

* <a href="/bulk/{stateAbbr}/v2">/bulk/{stateAbbr}/v2</a>

    * **stateAbbr** - Lowercase two-letter postal code abbreviation Default: none

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="bulk-api-bulk-upload">Bulk Upload</h1>

## upload_all_participants

<a id="opIdupload_all_participants"></a>

> Code samples

```shell
# You can also use wget
curl -X PUT /bulk/{stateAbbr}/v2/upload_all_participants/{filename} \
  -H 'Content-Type: text/plain' \
  -H 'Accept: application/json' \
  -H 'Content-Length: 6413' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`PUT /upload_all_participants/{filename}`

*Upload a CSV file of bulk participant data*

> Body parameter

```
string

```

<h3 id="upload_all_participants-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|filename|path|string|true|Name of file being uploaded|
|Content-Length|header|integer|true|Size in bytes of your file to be uploaded. A curl request will add this header by default when including a data or file parameter.|

<h3 id="upload_all_participants-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|File uploaded|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Access denied|None|
|411|[Length Required](https://tools.ietf.org/html/rfc7231#section-6.5.10)|Content-Length not provided|None|

<h3 id="upload_all_participants-responseschema">Response Schema</h3>

Status Code **201**

*Upload response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Will contain a data property with upload details.|
|»» upload_id|string|true|none|A unique upload_id for the successful upload.|

### Response Examples

> An upload of a single CSV file of all the participants

```json
{
  "data": {
    "upload_id": "0x8DA2EA86C4C2089"
  }
}
```

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

## upload (deprecated)

<a id="opIdupload (deprecated)"></a>

> Code samples

```shell
# You can also use wget
curl -X PUT /bulk/{stateAbbr}/v2/upload/{filename} \
  -H 'Content-Type: text/plain' \
  -H 'Accept: application/json' \
  -H 'Content-Length: 6413' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`PUT /upload/{filename}`

*Deprecated. This endpoint has been renamed to `upload_all_participants` and will be removed in a future version. Upload a CSV file of bulk participant data.*

> Body parameter

```
string

```

<h3 id="upload-(deprecated)-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|filename|path|string|true|Name of file being uploaded|
|Content-Length|header|integer|true|Size in bytes of your file to be uploaded. A curl request will add this header by default when including a data or file parameter.|

<h3 id="upload-(deprecated)-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|File uploaded|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Access denied|None|
|411|[Length Required](https://tools.ietf.org/html/rfc7231#section-6.5.10)|Content-Length not provided|None|

<h3 id="upload-(deprecated)-responseschema">Response Schema</h3>

Status Code **201**

*Upload response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Will contain a data property with upload details.|
|»» upload_id|string|true|none|A unique upload_id for the successful upload.|

### Response Examples

> An upload of a single CSV file of all the participants

```json
{
  "data": {
    "upload_id": "0x8DA2EA86C4C2089"
  }
}
```

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

