namespace RoadRegistry.BackOffice.Contracts.Uploads;

public sealed record UploadStatusRequest(string Identifier) : EndpointRequest<UploadStatusResponse>
{
}
