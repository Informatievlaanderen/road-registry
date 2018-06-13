namespace RoadRegistry.Projections
{
    using System.Threading.Tasks;

    public interface IOrganizationRetriever
    {
        Organization Get(string organizationId);
    }
}
