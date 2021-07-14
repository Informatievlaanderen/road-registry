namespace RoadRegistry.BackOffice.ExtractHost
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IStreetNameCache
    {
        Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds);
    }
}
