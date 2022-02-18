
<h1 id="duplicate-participation-api">Duplicate Participation API v2.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API where matching will occur

Base URLs:

* <a href="/match/v2">/match/v2</a>

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="duplicate-participation-api-match">Match</h1>

## Find matches

<a id="opIdFind matches"></a>

> Code samples

```shell
# You can also use wget
curl -X POST /match/v2/find_matches \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json' \
  -H 'From: string' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`POST /find_matches`

*Search for all matching participant records using de-identified data*

Searches all state databases for any participant records that are an exact match to the `lds_hash` of persons provided in the request.

> Body parameter

> An example request to query a single person, with values for all fields

```json
{
  "data": [
    {
      "lds_hash": "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
    }
  ]
}
```

<h3 id="find-matches-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|From|header|string|false|As in the HTTP/1.1 RFC, used for logging purposes as a means for identifying the source of invalid or unwanted requests. The interpretation of this field is that the request is being performed on behalf of the state government-affiliated person whose email address (or username) is specified here. It is not used for authentication or authorization.|
|data|body|[object]|true|none|
|» lds_hash|body|string|true|SHA-512 digest of participant's last name, DoB, and SSN. See docs/pprl.md for details|

> Example responses

> A query for a single person returning a single match

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "BCD2345",
            "state": "ea",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": "2021-01",
            "recent_benefit_months": [
              "2021-05",
              "2021-04",
              "2021-03"
            ],
            "protect_location": true
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for a single person returning no matches

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": []
      }
    ],
    "errors": []
  }
}
```

> A query for one person returning multiple matches

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "XYZ9876",
            "state": "eb",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": "2021-01",
            "recent_benefit_months": [
              "2021-05",
              "2021-04",
              "2021-03"
            ],
            "protect_location": true
          },
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for two persons returning one match for each person

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      },
      {
        "index": 1,
        "matches": [
          {
            "match_id": "BCD2345",
            "state": "ea",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": "2021-01",
            "recent_benefit_months": [
              "2021-05",
              "2021-04",
              "2021-03"
            ],
            "protect_location": true
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for two persons returning no matches for one person and a match for the other

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": []
      },
      {
        "index": 1,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for two persons returning a successful result for one person and an error for the other person

```json
{
  "data": {
    "results": [
      {
        "index": 1,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": [
      {
        "index": 0,
        "code": "XYZ",
        "title": "Internal Server Exception",
        "detail": "Unexpected Server Error. Please try again."
      }
    ]
  }
}
```

> An example response for an invalid request

```json
{
  "errors": [
    {
      "status": "400",
      "code": "XYZ",
      "title": "Bad Request",
      "detail": "Request payload exceeds maxiumum count"
    }
  ]
}
```

<h3 id="find-matches-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Successful response. Returns match response items.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request. Request body does not match the required format.|None|

<h3 id="find-matches-responseschema">Response Schema</h3>

Status Code **200**

*Match response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Either an errors or data property will be present in the response, but not both.|
|»» results|array|true|none|Array of query results. For every person provided in the request, a result is returned, even if no matches are found. If a query fails, the failure data will be in the errors array.|
|»»» index|integer|true|none|The index of the person that the result corresponds to, starting from 0. Index is derived from the implicit order of persons provided in the request.|
|»»» matches|array|true|none|none|
|»»»» match_id|string|true|none|Unique identifier for the match|
|»»»» state|string|true|none|State/territory two-letter postal abbreviation|
|»»»» case_id|string|true|none|Participant's state-specific case identifier. Can be the same for multiple participants.|
|»»»» participant_id|string|true|none|Participant's state-specific identifier. Is unique to the participant. Must not be social security number or any PII.|
|»»»» benefits_end_month|string|false|none|Participant's ending benefits month|
|»»»» recent_benefit_months|array|false|none|List of up to the last 3 months that participant received benefits, in descending order. Each month is formatted as ISO 8601 year and month. Does not include current benefit month.|
|»»»» protect_location|boolean|false|none|Location protection flag for vulnerable individuals. True values indicate that the individual’s location must be protected from disclosure to avoid harm to the individual. Apply the same protections to true and null values.|
|»» errors|array|true|none|Array of error objects corresponding to a person in the request. If a query for a single person fails, the failure data will display here. Note that a single person in a request could have multiple error items.|
|»»» index|integer|true|none|The index of the person that the result corresponds to, starting from 0. Index is derived from the implicit order of persons provided in the request.|
|»»» code|string|false|none|The application-specific error code|
|»»» title|string|false|none|The short, human-readable summary of the error, consistent across all occurrences of the error|
|»»» detail|string|false|none|The human-readable explanation specific to this occurrence of the error|
|» errors|array|false|none|Holds HTTP and other top-level errors. Either an errors or data property will be present in the response, but not both.|
|»» status|string|true|none|The HTTP status code|
|»» code|string|false|none|The application-specific error code|
|»» title|string|false|none|The short, human-readable summary of the error, consistent across all occurrences of the error|
|»» detail|string|false|none|The human-readable explanation specific to this occurrence of the error|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

