namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;

public class RoadNetworkTestBase : RoadRegistryTestBase
{
    protected readonly RoadNetworkTestData TestData;

    public RoadNetworkTestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        TestData = new RoadNetworkTestData(CustomizeTestData);
        TestData.CopyCustomizationsTo(ObjectProvider);
    }

    protected virtual void CustomizeTestData(Fixture fixture)
    { }

    public static IEnumerable<object[]> NonAdjacentLaneAttributesCases => RoadNetworkTestData.NonAdjacentLaneAttributesCases;
    public static IEnumerable<object[]> NonAdjacentSurfaceAttributesCases => RoadNetworkTestData.NonAdjacentSurfaceAttributesCases;
    public static IEnumerable<object[]> NonAdjacentWidthAttributesCases => RoadNetworkTestData.NonAdjacentWidthAttributesCases;
    public static IEnumerable<object[]> SelfOverlapsCases => RoadNetworkTestData.SelfOverlapsCases;
}
