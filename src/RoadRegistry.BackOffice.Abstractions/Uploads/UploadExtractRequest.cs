namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public record UploadExtractRequest(UploadExtractArchiveRequest Archive) : EndpointRequest<UploadExtractResponse>;
