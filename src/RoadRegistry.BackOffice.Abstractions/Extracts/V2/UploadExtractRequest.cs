namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public record UploadExtractRequest(Guid DownloadId, string ArchiveId) : EndpointRequest;
