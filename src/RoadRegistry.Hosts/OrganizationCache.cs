namespace RoadRegistry.Hosts
{
    using System.Collections.Concurrent;
    using BackOffice;
    using BackOffice.Abstractions.Organizations;
    using BackOffice.FeatureToggles;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Core;

    public class OrganizationCache : IOrganizationCache
    {
        private readonly EditorContext _editorContext;
        private readonly OrganizationDbaseRecordReader _organizationRecordReader;
        private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly ConcurrentDictionary<OrganizationId, OrganizationDetail> _cache = new();
        private readonly ILogger _logger;

        public OrganizationCache(
            EditorContext editorContext,
            RecyclableMemoryStreamManager manager,
            FileEncoding fileEncoding,
            UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
            IRoadRegistryContext roadRegistryContext,
            ILogger<OrganizationCache> logger)
        {
            _editorContext = editorContext;
            _organizationRecordReader = new OrganizationDbaseRecordReader(manager, fileEncoding);
            _useOvoCodeInChangeRoadNetworkFeatureToggle = useOvoCodeInChangeRoadNetworkFeatureToggle;
            _roadRegistryContext = roadRegistryContext;
            _logger = logger;
        }

        public async Task<OrganizationDetail?> FindByIdOrOvoCodeAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            if (OrganizationId.IsSystemValue(organizationId))
            {
                var translation = Organization.PredefinedTranslations.FromSystemValue(organizationId);

                return new OrganizationDetail
                {
                    Code = translation.Identifier,
                    Name = translation.Name
                };
            }

            if (_cache.TryGetValue(organizationId, out var cachedOrganization))
            {
                return cachedOrganization;
            }

            var organization = await FindOrganizationAsync(organizationId, cancellationToken);
            return _cache.GetOrAdd(organizationId, organization);
        }

        private async Task<OrganizationDetail> FindOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            if (OrganizationOvoCode.AcceptsValue(organizationId))
            {
                if (_useOvoCodeInChangeRoadNetworkFeatureToggle.FeatureEnabled)
                {
                    return await GetById(organizationId, cancellationToken);
                }

                return await FindByMappedOvoCode(new OrganizationOvoCode(organizationId), cancellationToken);
            }

            return await GetById(organizationId, cancellationToken);
        }

        private async Task<OrganizationDetail> GetById(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            var organization = await _roadRegistryContext.Organizations.FindAsync(organizationId, cancellationToken);
            if (organization is not null)
            {
                return new OrganizationDetail
                {
                    Code = organizationId,
                    Name = organization.Translation.Name,
                    OvoCode = organization.OvoCode
                };
            }

            return null;
        }

        private async Task<OrganizationDetail> FindByMappedOvoCode(OrganizationOvoCode ovoCode, CancellationToken cancellationToken)
        {
            var organizationRecords = await _editorContext.Organizations.ToListAsync(cancellationToken);
            var organizationDetail = organizationRecords
                .Select(organizationRecord => _organizationRecordReader.Read(organizationRecord.DbaseRecord, organizationRecord.DbaseSchemaVersion))
                .SingleOrDefault(sod => sod.OvoCode == ovoCode);

            if (organizationDetail is not null)
            {
                return organizationDetail;
            }

            _logger.LogError($"Could not find a mapping to an organization for OVO-code {ovoCode}");
            return null;
        }
    }
}
