namespace RoadRegistry.BackOffice.Abstractions.Organizations;

public sealed record GetOrganizationsResponse(ICollection<OrganizationDetail> Organizations) : EndpointResponse;
