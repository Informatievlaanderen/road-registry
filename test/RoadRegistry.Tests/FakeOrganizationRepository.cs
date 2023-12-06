namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;

    public class FakeOrganizationRepository: IOrganizationRepository
    {
        public Task<OrganizationDetail> FindByIdOrOvoCodeAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(OrganizationDetail.FromCode(organizationId));
        }
    }
}
