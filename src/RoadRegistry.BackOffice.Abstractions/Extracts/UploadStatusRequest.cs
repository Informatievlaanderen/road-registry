namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record UploadStatusRequest(string Identifier) : EndpointRequest<UploadStatusResponse>
{
}
