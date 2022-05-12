
<h1 id="bulk-api">Bulk API v2.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API for performing bulk uploads

Base URLs:

* <a href="/bulk/{stateAbbr}/v2">/bulk/{stateAbbr}/v2</a>

    * **stateAbbr** - Lowercase two-letter postal code abbreviation Default: none

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="bulk-api-default">Default</h1>

## put__upload_all_participants_{filename}

> Code samples

```shell
# You can also use wget
curl -X PUT /bulk/{stateAbbr}/v2/upload_all_participants/{filename} \
  -H 'Accept: application/json' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`PUT /upload_all_participants/{filename}`

> Example responses

> An upload of a single CSV file of all the participants

```json
{
  "data": {
    "upload_id": "0x8DA2EA86C4C2089"
  }
}
```

<h3 id="put__upload_all_participants_{filename}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|File uploaded|Inline|

<h3 id="put__upload_all_participants_{filename}-responseschema">Response Schema</h3>

Status Code **201**

*Upload response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Will contain a data property with upload details.|
|»» upload_id|string|true|none|A unique upload_id for the successful upload.|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

## put__upload_{filename}

> Code samples

```shell
# You can also use wget
curl -X PUT /bulk/{stateAbbr}/v2/upload/{filename} \
  -H 'Accept: application/json' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`PUT /upload/{filename}`

A Deprecated endpoint.

> Example responses

> An upload of a single CSV file of all the participants

```json
{
  "data": {
    "upload_id": "0x8DA2EA86C4C2089"
  }
}
```

<h3 id="put__upload_{filename}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|(Deprecated) File uploaded|Inline|

<h3 id="put__upload_{filename}-responseschema">Response Schema</h3>

Status Code **201**

*Upload response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Will contain a data property with upload details.|
|»» upload_id|string|true|none|A unique upload_id for the successful upload.|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

