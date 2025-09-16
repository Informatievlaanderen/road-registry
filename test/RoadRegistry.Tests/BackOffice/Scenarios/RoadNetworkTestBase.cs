namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice.FeatureToggles;

public class RoadNetworkTestBase : RoadRegistryTestBase
{
    protected readonly RoadNetworkTestData TestData;

    protected RoadNetworkTestBase(ITestOutputHelper testOutputHelper, UseExtractsV2FeatureToggle useExtractsV2FeatureToggle = null)
        : base(testOutputHelper, useExtractsV2FeatureToggle: useExtractsV2FeatureToggle)
    {
        TestData = new RoadNetworkTestData();
        TestData.CopyCustomizationsTo(ObjectProvider);
    }

    public static IEnumerable<object[]> NonAdjacentLaneAttributesCases => RoadNetworkTestData.NonAdjacentLaneAttributesCases;
    public static IEnumerable<object[]> NonAdjacentSurfaceAttributesCases => RoadNetworkTestData.NonAdjacentSurfaceAttributesCases;
    public static IEnumerable<object[]> NonAdjacentWidthAttributesCases => RoadNetworkTestData.NonAdjacentWidthAttributesCases;
    public static IEnumerable<object[]> SelfOverlapsCases => RoadNetworkTestData.SelfOverlapsCases;
}
