namespace RoadRegistry.BackOffice.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOrganizations
    {
        Task<Organization> TryGet(OrganizationId id, CancellationToken ct = default);
    }
}
