namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda
{
    using Ductus.FluentDocker.Builders;

    internal static class FeatureCompareDockerContainerBuilder
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
}
