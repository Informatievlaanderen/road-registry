# Introduction

This document describes the api integration with an external party (initially GRB). The party can download an arbitrary portion of the road network and upload changes made to that arbitrary portion of the road network.

## Download Request

Composing a package with an arbitrary portion of the road network takes time. Therefor it's necessary to put in a request that one wants to download a portion of the road network. By specifying a boundary geometry one can indicate the portion one is interested in. All road segments that are contained within this boundary or intersect with this boundary are selected. We also select the road nodes attached to them, the grade separated junctions for which both upper and lower road segments are in the selected set of road segments. Naturally, any data associated with the road segments, such as the european, national, numbered roads they are part of, as well as the dynamic segments such as lanes, widths and surfaces, will become part of the requested download package. We will also include all static data we offer in a full download of the road network.

### Request

```
POST /api/v1/downloadrequests
Content-Type: application/json

{
  "id": "GRB_20210301_003:237",
  "boundaryGeometry": "POLYGON((0.5 0.5,5 0,5 5,0 5,0.5 0.5), (1.5 1,4 3,4 1,1.5 1))"
}
```

| Field | Data Type | Constraints | Description |
|----|----|----|----|
| Id | String | Required (NotNull, NotEmpty) | External Party's identifier to associate with the request |
| BoundaryGeometry | String | Required, WKT Polygon (OGC) | Portion of the road network to select |

### Response

#### Accepted

In case we were able to accept the request, we return the request identifier, which can be used to poll for the download package. It's important for callers to remember this identifier, because it will act as a means to download the package with the requested data.

```
202 Accepted
Content-Type: application/json

{
  "downloadId": "opaque-url-safe-string-identifying-the-future-download"
}
```

#### Bad Request

In case the `Id` or `BoundaryGeometry` was not specified, or `BoundaryGeometry` was not a polygon, we return an error indicating as much.

```
400 Bad Request
Content-Type: application/problem+json

{
  "type": "urn:road-registry:v1:bad-request",
  "title": "Your request did not match our expectations",
  "errors": [ "IdMissing", "BoundaryGeometryMissing", "BoundaryGeometryMismatch" ]
}
```

> Note: We use https://tools.ietf.org/html/rfc7807 to communicate errors.

## Download

Once all the requested data has been packaged, the external party can poll the following URL to download the package. The download identifier returned previously can be used to perform the actual download.

### Request

```
GET /api/v1/download/{download-id}
```

| Parameter | Data Type | Constraints | Description |
|----|----|----|----|
| DownloadId | String | Required | Identifies the package to download |

### Response

In case the package for the request is available, we'll start streaming the zip file as a response.

```
200 Ok
Content-Type: application/zip
Content-Disposition: attachment; filename="download.zip";
Transfer-Encoding: chunked

/* zipped content goes here */
```

### Response

In case the package for the request is still unavailable, we'll indicate when is a good time to do the next poll.

```
404 Not Found
Retry-After: 120
```

> Note: Retry-After time is expressed in seconds.

## Upload

Once the external party has performed the necessary changes on the requested data, they can upload those changes as a package. By specifying their id we can associate a download request with an upload.

### Request

```
POST /api/v1/uploads/
Content-Length: 10294286
Content-Type: multipart/form-data;boundary=---------------------------107813337518460042292291642725

---------------------------107813337518460042292291642725
Content-Type: text/plain
Content-Disposition: form-data; name="id"

GRB_20210301_003:237
---------------------------107813337518460042292291642725
Content-Type: application/zip
Content-Disposition: form-data; name="archive"; filename="upload.zip";

/* zipped content goes here */
```

### Response

In case we accept the upload, we return an upload identifier.

```
202 Accepted
Content-Type: application/json

{
  "uploadId": "opaque-url-safe-string-identifying-the-upload"
}
```

## Upload Status

Once the upload has been performed, the external party can poll to find out if, when and how the upload was handled.

### Request

```
GET /api/v1/upload/{upload-id}/status
```

### Response

```
200 Ok
Content-Type: application/json

{
  "uploadId": "opaque-url-safe-string-identifying-the-upload"
  "status": "Queued|Accepted|Rejected"
}
```
