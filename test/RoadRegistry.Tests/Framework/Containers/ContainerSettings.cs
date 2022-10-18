namespace RoadRegistry.Tests.Framework.Containers;

public class ContainerSettings
{
    public string[] EnvironmentVariables { get; set; } = Array.Empty<string>();
    public string Name { get; set; }

    public PortBinding[] PortBindings { get; set; } = Array.Empty<PortBinding>();

    public bool RemoveContainer { get; set; } = true;

    public bool StopContainer { get; set; } = true;
}