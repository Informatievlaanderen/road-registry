namespace RoadRegistry.BackOffice.Api.Infrastructure.FeatureToggles
{
    using FeatureToggle;

    public sealed class UseApiKeyAuthenticationToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseApiKeyAuthenticationToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
