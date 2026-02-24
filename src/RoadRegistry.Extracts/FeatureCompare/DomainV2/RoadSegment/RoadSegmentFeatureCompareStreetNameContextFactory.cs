namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Infrastructure;

public interface IRoadSegmentFeatureCompareStreetNameContextFactory
{
    Task<IRoadSegmentFeatureCompareStreetNameContext> Create(
        ICollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures,
        CancellationToken cancellationToken);
}

public class RoadSegmentFeatureCompareStreetNameContextFactory: IRoadSegmentFeatureCompareStreetNameContextFactory
{
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentFeatureCompareStreetNameContextFactory(IStreetNameCache streetNameCache)
    {
        _streetNameCache = streetNameCache;
    }

    public async Task<IRoadSegmentFeatureCompareStreetNameContext> Create(
        ICollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures,
        CancellationToken cancellationToken)
    {
        var usedStreetNameIds = changeFeatures
            .Where(x => x.Attributes.LeftSideStreetNameId > 0)
            .Select(x => x.Attributes.LeftSideStreetNameId.Value)
            .Concat(changeFeatures
                .Where(x => x.Attributes.RightSideStreetNameId > 0)
                .Select(x => x.Attributes.RightSideStreetNameId.Value))
            .Select(x => x.ToInt32())
            .Distinct()
            .ToList();

        if (!usedStreetNameIds.Any())
        {
            return new RoadSegmentFeatureCompareStreetNameContext([], []);
        }

        var usedStreetNames = await _streetNameCache.GetAsync(usedStreetNameIds, cancellationToken);
        var renamedStreetNameIds = await _streetNameCache.GetRenamedIdsAsync(usedStreetNameIds, cancellationToken);

        return new RoadSegmentFeatureCompareStreetNameContext(usedStreetNames, renamedStreetNameIds);
    }
}
