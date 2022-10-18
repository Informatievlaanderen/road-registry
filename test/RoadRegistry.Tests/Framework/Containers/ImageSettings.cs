namespace RoadRegistry.Tests.Framework.Containers;

public class ImageSettings
{
    public string FullyQualifiedName => Registry + "/" + TagQualifiedName;

    public string Name { get; set; }
    public string Registry { get; set; }
    public string RegistryQualifiedName => Registry + "/" + Name;

    public string Tag { get; set; } = "latest";

    public string TagQualifiedName => Name + ":" + Tag;
}
