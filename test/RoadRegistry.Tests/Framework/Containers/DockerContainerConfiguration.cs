namespace RoadRegistry.Tests.Framework.Containers;

public class DockerContainerConfiguration
{
    public ContainerSettings Container { get; set; }
    public ImageSettings Image { get; set; }

    public Func<int, Task<TimeSpan>> WaitUntilAvailable { get; set; } = attempts => Task.FromResult(TimeSpan.Zero);
}
