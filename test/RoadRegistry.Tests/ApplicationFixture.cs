namespace RoadRegistry.Tests;

using AutoFixture;
using BackOffice;

public abstract class ApplicationFixture
{
    protected ApplicationFixture()
    {
        ObjectProvider = FixtureFactory.Create();
        ObjectProvider.CustomizePolylineM();
        ObjectProvider.CustomizeRoadSegmentId();
        ObjectProvider.CustomizeRoadSegmentCategory();
        ObjectProvider.CustomizeRoadSegmentMorphology();
        ObjectProvider.CustomizeRoadSegmentStatus();
        ObjectProvider.CustomizeRoadSegmentAccessRestriction();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentWidth();
        ObjectProvider.CustomizeOrganizationId();
    }

    public Fixture ObjectProvider { get; }
}
