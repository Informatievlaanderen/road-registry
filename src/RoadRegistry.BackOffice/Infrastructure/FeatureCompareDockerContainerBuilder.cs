namespace RoadRegistry.BackOffice.Infrastructure;

using Ductus.FluentDocker.Builders;

public static class FeatureCompareDockerContainerBuilder
{
    public static ContainerBuilder Default => new Builder()
        .UseContainer()
        .WithName("basisregisters-road-registry-feature-compare")
        .UseImage("road-registry/feature-compare:latest")
        .IsWindowsImage()
        .ReuseIfExists()
        .KeepContainer()
        .KeepRunning();
}
