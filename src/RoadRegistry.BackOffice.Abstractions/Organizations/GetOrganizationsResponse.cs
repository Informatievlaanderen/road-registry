namespace RoadRegistry.BackOffice.Abstractions.Organizations;

using RoadRegistry.Infrastructure;

public sealed record GetOrganizationsResponse(ICollection<OrganizationDetail> Organizations) : EndpointResponse;
