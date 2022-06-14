namespace RoadRegistry.Hosts
{
    using FeatureToggle;

    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseSomeFeatureV2 { get; set; }
    }

    public class UseSomeFeatureV2Toggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseSomeFeatureV2Toggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
