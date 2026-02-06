namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AggregateTests;

public class RoadNetworkTestBase : RoadRegistryTestBase
{
    protected readonly RoadNetworkTestData TestData;

    protected RoadNetworkTestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        TestData = new RoadNetworkTestData();
        TestData.CopyCustomizationsTo(ObjectProvider);
        new RoadNetworkTestDataV2().CopyCustomizationsTo(ObjectProvider);
    }

    public static IEnumerable<object[]> NonAdjacentLaneAttributesCases => RoadNetworkTestData.NonAdjacentLaneAttributesCases;
    public static IEnumerable<object[]> NonAdjacentSurfaceAttributesCases => RoadNetworkTestData.NonAdjacentSurfaceAttributesCases;
    public static IEnumerable<object[]> NonAdjacentWidthAttributesCases => RoadNetworkTestData.NonAdjacentWidthAttributesCases;
    public static IEnumerable<object[]> SelfOverlapsCases => RoadNetworkTestData.SelfOverlapsCases;
}
