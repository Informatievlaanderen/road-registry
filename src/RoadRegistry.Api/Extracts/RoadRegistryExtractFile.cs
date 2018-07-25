namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.IO;

    public class RoadRegistryExtractFile
    {
        public RoadRegistryExtractFile(string name, Stream content)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Content = content;
        }

        public string Name { get; }
        public Stream Content { get; }
    }
}
