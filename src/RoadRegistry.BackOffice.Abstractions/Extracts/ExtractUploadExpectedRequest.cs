namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractUploadExpectedRequest(DownloadId DownloadId, bool UploadExpected) : EndpointRequest<ExtractUploadExpectedResponse> {}
