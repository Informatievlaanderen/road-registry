namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Uploads;

public record UploadExtractRequest(string DownloadId, UploadExtractArchiveRequest Archive, Guid? TicketId) : EndpointRequest<UploadExtractResponse>;
