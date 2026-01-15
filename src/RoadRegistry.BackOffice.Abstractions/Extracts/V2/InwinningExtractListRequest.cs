namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record InwinningExtractListRequest(string OrganizationCode) : EndpointRequest<ExtractListResponse>
{
}
