namespace RoadRegistry.Hosts
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.FeatureToggles;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class OrganizationCache : IOrganizationCache
    {
        private readonly EditorContext _editorContext;
        private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly ConcurrentDictionary<OrganizationId, OrganizationDetail> _cache = new();
        private readonly ILogger _logger;

        public OrganizationCache(
            EditorContext editorContext,
            UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
            IRoadRegistryContext roadRegistryContext,
            ILogger<OrganizationCache> logger)
        {
            _editorContext = editorContext;
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
                    OvoCode = organization.OvoCode,
                    KboNumber = organization.KboNumber
                };
            }

            return null;
        }

        private async Task<OrganizationDetail> FindByMappedOvoCode(OrganizationOvoCode ovoCode, CancellationToken cancellationToken)
        {
            var organizationRecords = await _editorContext.OrganizationsV2
                .Where(x => x.OvoCode == ovoCode)
                .ToListAsync(cancellationToken);

            if (organizationRecords.Count > 1)
            {
                throw new Exception($"Multiple organizations found in cache for OVO-code '{ovoCode}' (Ids: {string.Join(", ", organizationRecords.Select(x => x.Code))})");
            }

            var organizationRecord = organizationRecords.SingleOrDefault();
            if (organizationRecord is not null)
            {
                return new OrganizationDetail
                {
                    Code = new OrganizationId(organizationRecord.Code),
                    Name = new OrganizationName(organizationRecord.Name),
                    OvoCode = OrganizationOvoCode.FromValue(organizationRecord.OvoCode),
                    KboNumber = OrganizationKboNumber.FromValue(organizationRecord.KboNumber)
                };
            }

            _logger.LogError($"Could not find a mapping to an organization for OVO-code {ovoCode}");
            return null;
        }
    }
}
