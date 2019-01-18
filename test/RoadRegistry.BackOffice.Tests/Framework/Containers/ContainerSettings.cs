namespace RoadRegistry.BackOffice.Framework.Containers
{
    public class ContainerSettings
    {
        public string Name { get; set; }

        public PortBinding[] PortBindings { get; set; } = new PortBinding[0];

        public string[] EnvironmentVariables { get; set; } = new string[0];

        public bool StopContainer { get; set; } = true;

        public bool RemoveContainer { get; set; } = true;
    }
}