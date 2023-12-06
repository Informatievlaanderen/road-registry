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

    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly EditorContext _editorContext;
        private readonly OrganizationDbaseRecordReader _organizationRecordReader;
        private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly ConcurrentDictionary<OrganizationId, OrganizationDetail> _cache = new ();
        private readonly ILogger _logger;

        public OrganizationRepository(
            EditorContext editorContext,
            RecyclableMemoryStreamManager manager,
            FileEncoding fileEncoding,
            UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
            IRoadRegistryContext roadRegistryContext,
            ILogger<OrganizationRepository> logger)
        {
            _editorContext = editorContext;
            _organizationRecordReader = new OrganizationDbaseRecordReader(manager, fileEncoding);
            _useOvoCodeInChangeRoadNetworkFeatureToggle = useOvoCodeInChangeRoadNetworkFeatureToggle;
            _roadRegistryContext = roadRegistryContext;
            _logger = logger;
        }
        
        public async Task<OrganizationDetail?> FindByIdOrOvoCodeAsync(OrganizationId organizationId, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(organizationId, out var cachedOrganization))
            {
                return cachedOrganization;
            }

            var organization = await FindOrganizationAsync(organizationId, cancellationToken);
            return _cache.GetOrAdd(organizationId, organization);
        }

        //TODO-rik add unit test
        private async Task<OrganizationDetail> FindOrganizationAsync(string code, CancellationToken cancellationToken)
        {
            if (OrganizationOvoCode.AcceptsValue(code))
            {
                if (_useOvoCodeInChangeRoadNetworkFeatureToggle.FeatureEnabled)
                {
                    var organizationRecord = await _editorContext.Organizations.SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
                    if (organizationRecord is not null)
                    {
                        return _organizationRecordReader.Read(organizationRecord.DbaseRecord, organizationRecord.DbaseSchemaVersion);
                    }

                    return null;
                }

                var organizationRecords = await _editorContext.Organizations.ToListAsync(cancellationToken);
                var organizationDetails = organizationRecords.Select(organization => _organizationRecordReader.Read(organization.DbaseRecord, organization.DbaseSchemaVersion)).ToList();

                var ovoCode = new OrganizationOvoCode(code);
                var organizationDetail = organizationDetails.SingleOrDefault(sod => sod.OvoCode == ovoCode);
                if (organizationDetail is not null)
                {
                    return organizationDetail;
                }

                _logger.LogError($"Could not find a mapping to an organization for OVO-code {ovoCode}");
                return null;
            }

            var organizationId = new OrganizationId(code);
            var organization = await _roadRegistryContext.Organizations.FindAsync(organizationId, cancellationToken);
            return organization is not null
                ? new OrganizationDetail
                {
                    Code = organizationId,
                    Name = organization.Translation.Name,
                    OvoCode = organization.OvoCode
                }
                : null;
        }
    }
}
