namespace RoadRegistry.Tests
{
    using Extracts.FeatureCompare.DomainV2.RoadSegment;
    using NetTopologySuite.Geometries;
    using RoadRegistry.BackOffice;
    using RoadRegistry.Infrastructure;

    public class FakeGrbOgcApiFeaturesDownloader : IGrbOgcApiFeaturesDownloader
    {
        private readonly ICollection<Geometry> _geometries = [];

        public FakeGrbOgcApiFeaturesDownloader()
        {
        }

        public FakeGrbOgcApiFeaturesDownloader(IEnumerable<Geometry> geometries)
        {
            _geometries = geometries.ToList();
        }

        public async Task<IReadOnlyList<OgcFeature>> DownloadFeaturesAsync(IEnumerable<string> collectionIds, Envelope boundingBox, int srid, CancellationToken cancellationToken)
        {
            return collectionIds
                .SelectMany(collectionId => _geometries.Select(x => new OgcFeature(collectionId, null, x, null)))
                .ToList()
                .AsReadOnly();
        }
    }
    public class FakeOrganizationCache : IOrganizationCache
    {
        private readonly Dictionary<OrganizationId, OrganizationDetail> _organizations = new();

        public Task<OrganizationDetail> FindByIdOrOvoCodeOrKboNumberAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            if (OrganizationId.IsSystemValue(organizationId))
            {
                var translation = OrganizationName.PredefinedTranslations.FromSystemValue(organizationId);

                return Task.FromResult(new OrganizationDetail
                {
                    Code = translation.Identifier,
                    Name = translation.Name
                });
            }

            if (_organizations.TryGetValue(organizationId, out var organization))
            {
                return Task.FromResult(organization);
            }

            return Task.FromResult(OrganizationDetail.FromCode(organizationId));
        }

        public FakeOrganizationCache Seed(OrganizationId organizationId, OrganizationDetail organization)
        {
            _organizations[organizationId] = organization;
            return this;
        }
    }
}
