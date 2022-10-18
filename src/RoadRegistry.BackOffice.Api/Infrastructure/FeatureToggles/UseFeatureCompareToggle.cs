namespace RoadRegistry.BackOffice.Api.Infrastructure.FeatureToggles;

using FeatureToggle;

public sealed class UseFeatureCompareToggle : IFeatureToggle
{
    public UseFeatureCompareToggle(bool featureEnabled)
    {
        FeatureEnabled = featureEnabled;
    }

    public bool FeatureEnabled { get; }
}