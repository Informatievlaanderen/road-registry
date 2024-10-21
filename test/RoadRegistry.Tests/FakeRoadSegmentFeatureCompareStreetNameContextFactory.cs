namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.FeatureCompare.Translators;

public class FakeRoadSegmentFeatureCompareStreetNameContextFactory: IRoadSegmentFeatureCompareStreetNameContextFactory
{
    private readonly IRoadSegmentFeatureCompareStreetNameContext _context;

    public FakeRoadSegmentFeatureCompareStreetNameContextFactory()
    {
        _context = new FakeRoadSegmentFeatureCompareStreetNameContext();
    }

    public Task<IRoadSegmentFeatureCompareStreetNameContext> Create(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context);
    }
}

public class FakeRoadSegmentFeatureCompareStreetNameContext : IRoadSegmentFeatureCompareStreetNameContext
{
    public bool IsRemoved(StreetNameLocalId id)
    {
        return false;
    }

    public bool Exists(StreetNameLocalId id)
    {
        return true;
    }

    public bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId)
    {
        renamedToId = default;
        return false;
    }
}
