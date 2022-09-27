namespace RoadRegistry.Framework.Containers;

public class ContainerSettings
{
    public string Name { get; set; }

    public PortBinding[] PortBindings { get; set; } = Array.Empty<PortBinding>();

    public string[] EnvironmentVariables { get; set; } = Array.Empty<string>();

    public bool StopContainer { get; set; } = true;

    public bool RemoveContainer { get; set; } = true;
}
