namespace RoadRegistry.Projections
{
    using System.Threading.Tasks;

    public interface IOrganisationRetreiver
    {
        Organisation Get(string organisationId);
    }

    public class Organisation
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
