namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.FeatureCompare.V2.Models;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public class FakeRoadSegmentFeatureCompareStreetNameContextFactoryV2: IRoadSegmentFeatureCompareStreetNameContextFactory
{
    private readonly IRoadSegmentFeatureCompareStreetNameContext _context;

    public FakeRoadSegmentFeatureCompareStreetNameContextFactoryV2()
    {
        _context = new FakeRoadSegmentFeatureCompareStreetNameContextV2();
    }

    public Task<IRoadSegmentFeatureCompareStreetNameContext> Create(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context);
    }
}

public class FakeRoadSegmentFeatureCompareStreetNameContextV2 : IRoadSegmentFeatureCompareStreetNameContext
{
    public bool IsRemoved(StreetNameLocalId id)
    {
        return false;
    }

    public bool IsValid(StreetNameLocalId id)
    {
        return true;
    }

    public bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId)
    {
        renamedToId = default;
        return false;
    }
}
