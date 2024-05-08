namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record UploadHealthCheckRequest : EndpointRequest<UploadHealthCheckResponse>
{
    public required Guid TicketId { get; init; }
}
