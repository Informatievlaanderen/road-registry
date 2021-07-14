namespace RoadRegistry.BackOffice.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IStreetNameCache
    {
        Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds);
    }
}
