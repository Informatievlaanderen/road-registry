namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public record UploadExtractRequest(UploadExtractArchiveRequest Archive, Guid? TicketId) : EndpointRequest<UploadExtractResponse>;
