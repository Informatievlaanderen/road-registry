namespace RoadRegistry.Tests.BackOffice.Scenarios;

public class RoadNetworkTestBase : RoadRegistryTestBase
{
    protected readonly RoadNetworkTestData TestData = new();

    public RoadNetworkTestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        TestData.CopyCustomizationsTo(ObjectProvider);
    }

    public static IEnumerable<object[]> NonAdjacentLaneAttributesCases => RoadNetworkTestData.NonAdjacentLaneAttributesCases;
    public static IEnumerable<object[]> NonAdjacentSurfaceAttributesCases => RoadNetworkTestData.NonAdjacentSurfaceAttributesCases;
    public static IEnumerable<object[]> NonAdjacentWidthAttributesCases => RoadNetworkTestData.NonAdjacentWidthAttributesCases;
    public static IEnumerable<object[]> SelfOverlapsCases => RoadNetworkTestData.SelfOverlapsCases;
}