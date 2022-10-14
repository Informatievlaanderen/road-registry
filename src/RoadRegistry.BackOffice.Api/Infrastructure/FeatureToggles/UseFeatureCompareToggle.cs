namespace RoadRegistry.BackOffice.Api.Infrastructure.FeatureToggles
{
    using FeatureToggle;

    public sealed class UseFeatureCompareToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseFeatureCompareToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
