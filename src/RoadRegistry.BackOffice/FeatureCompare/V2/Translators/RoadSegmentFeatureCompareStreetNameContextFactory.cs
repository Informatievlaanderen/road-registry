﻿namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;

public interface IRoadSegmentFeatureCompareStreetNameContextFactory
{
    Task<IRoadSegmentFeatureCompareStreetNameContext> Create(
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures,
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
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures,
        CancellationToken cancellationToken)
    {
        var usedStreetNameIds = changeFeatures
            .Where(x => x.Attributes.LeftStreetNameId > 0)
            .Select(x => x.Attributes.LeftStreetNameId)
            .Concat(changeFeatures
                .Where(x => x.Attributes.RightStreetNameId > 0)
                .Select(x => x.Attributes.RightStreetNameId))
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
