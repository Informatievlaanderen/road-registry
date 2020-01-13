namespace RoadRegistry.BackOffice.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOrganizations
    {
        Task<bool> Exists(OrganizationId id, CancellationToken ct = default);
    }
}
