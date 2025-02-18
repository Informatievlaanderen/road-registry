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
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly ConcurrentDictionary<OrganizationId, OrganizationDetail> _cache = new();
        private readonly ILogger _logger;

        public OrganizationCache(
            EditorContext editorContext,
            IRoadRegistryContext roadRegistryContext,
            ILogger<OrganizationCache> logger)
        {
            _editorContext = editorContext;
            _roadRegistryContext = roadRegistryContext;
            _logger = logger;
        }

        public async Task<OrganizationDetail?> FindByIdOrOvoCodeOrKboNumberAsync(OrganizationId organizationId, CancellationToken cancellationToken)
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
                return await GetById(organizationId, cancellationToken)
                    ?? await FindByMappedOvoCode(new OrganizationOvoCode(organizationId), cancellationToken);
            }

            if (OrganizationKboNumber.AcceptsValue(organizationId))
            {
                return await FindByKboNumber(new OrganizationKboNumber(organizationId), cancellationToken);
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
            var organizationCodes = await _editorContext.OrganizationsV2
                .Where(x => x.OvoCode == ovoCode)
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            if (organizationCodes.Count > 1)
            {
                throw new Exception($"Multiple organizations found in cache for OVO-code '{ovoCode}' (Ids: {string.Join(", ", organizationCodes)})");
            }

            var organizationCode = organizationCodes.SingleOrDefault();
            if (organizationCode is null)
            {
                _logger.LogError($"Could not find a mapping to an organization for OVO-code {ovoCode}");
                return null;
            }

            return await GetById(new OrganizationId(organizationCode), cancellationToken);
        }

        private async Task<OrganizationDetail> FindByKboNumber(OrganizationKboNumber kboNumber, CancellationToken cancellationToken)
        {
            var organizationCodes = await _editorContext.OrganizationsV2
                .Where(x => x.KboNumber == kboNumber)
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            if (organizationCodes.Count > 1)
            {
                throw new Exception($"Multiple organizations found in cache for KBO-number '{kboNumber}' (Ids: {string.Join(", ", organizationCodes)})");
            }

            var organizationCode = organizationCodes.SingleOrDefault();
            if (organizationCode is null)
            {
                _logger.LogError($"Could not find a mapping to an organization for KBO-number {kboNumber}");
                return null;
            }

            return await GetById(new OrganizationId(organizationCode), cancellationToken);
        }
    }
}
