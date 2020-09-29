namespace RoadRegistry.Wms.Projections
{
    using System.Threading.Tasks;
    using Syndication.Schema;

    public class MunicipalityCacheStub : IMunicipalityCache
    {
        private readonly MunicipalityRecord _stubbedValue;

        public MunicipalityCacheStub(MunicipalityRecord stubbedValue = null)
        {
            _stubbedValue = stubbedValue;
        }

        public Task<MunicipalityRecord> Get(string nisCode)
        {
            return Task.FromResult(_stubbedValue);
        }
    }
}
