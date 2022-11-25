namespace RoadRegistry.BackOffice;

using FeatureToggle;

public abstract record ApplicationFeatureToggle(bool FeatureEnabled) : IFeatureToggle
{
}
