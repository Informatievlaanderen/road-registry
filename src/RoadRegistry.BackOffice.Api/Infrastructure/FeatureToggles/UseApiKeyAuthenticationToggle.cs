namespace RoadRegistry.BackOffice.Api.Infrastructure.FeatureToggles;

using FeatureToggle;

public sealed class UseApiKeyAuthenticationToggle : IFeatureToggle
{
    public UseApiKeyAuthenticationToggle(bool featureEnabled)
    {
        FeatureEnabled = featureEnabled;
    }

    public bool FeatureEnabled { get; }
}