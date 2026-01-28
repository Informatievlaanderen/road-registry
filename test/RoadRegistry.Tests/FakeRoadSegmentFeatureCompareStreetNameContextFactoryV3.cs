namespace RoadRegistry.Tests;

using Extracts.FeatureCompare.DomainV2;
using Extracts.FeatureCompare.DomainV2.RoadSegment;

public class FakeRoadSegmentFeatureCompareStreetNameContextFactoryV3: IRoadSegmentFeatureCompareStreetNameContextFactory
{
    private readonly IRoadSegmentFeatureCompareStreetNameContext _context;

    public FakeRoadSegmentFeatureCompareStreetNameContextFactoryV3()
    {
        _context = new FakeRoadSegmentFeatureCompareStreetNameContextV3();
    }

    public Task<IRoadSegmentFeatureCompareStreetNameContext> Create(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context);
    }
}

public class FakeRoadSegmentFeatureCompareStreetNameContextV3 : IRoadSegmentFeatureCompareStreetNameContext
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
