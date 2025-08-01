namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record ExtractListRequest(string OrganizationCode) : EndpointRequest<ExtractListResponse>
{
}
