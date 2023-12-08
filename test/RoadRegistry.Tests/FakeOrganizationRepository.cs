namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;

    public class FakeOrganizationRepository: IOrganizationRepository
    {
        private readonly Dictionary<OrganizationId, OrganizationDetail> _organizations = new();

        public Task<OrganizationDetail> FindByIdOrOvoCodeAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            if (_organizations.TryGetValue(organizationId, out var organization))
            {
                return Task.FromResult(organization);
            }

            return Task.FromResult(OrganizationDetail.FromCode(organizationId));
        }

        public FakeOrganizationRepository Seed(OrganizationId organizationId, OrganizationDetail organization)
        {
            _organizations[organizationId] = organization;
            return this;
        }
    }
}
