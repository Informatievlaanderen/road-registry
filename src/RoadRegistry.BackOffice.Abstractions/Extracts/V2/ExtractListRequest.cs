namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record ExtractListRequest(string? OrganizationCode, int PageIndex, int PageSize = 100) : EndpointRequest<ExtractListResponse>
{
}
