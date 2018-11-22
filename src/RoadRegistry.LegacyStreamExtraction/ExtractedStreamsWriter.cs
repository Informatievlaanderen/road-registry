namespace RoadRegistry.LegacyStreamExtraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RoadRegistry.Events;

    internal class ExtractedStreamsWriter
    {
        public ExtractedStreamsWriter(FileInfo output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public FileInfo Output { get; }

        public async Task WriteAsync(
            IEnumerable<ImportedOrganization> organizations,
            IEnumerable<ImportedRoadNode> nodes,
            IEnumerable<ImportedRoadSegment> segments,
            IEnumerable<ImportedGradeSeparatedJunction> junctions,
            IEnumerable<ImportedReferencePoint> points)
        {
            using (var fileStream = Output.OpenWrite())
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("streams.json", CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                using (var writer = new StreamJsonTextWriter(new JsonTextWriter(new StreamWriter(entryStream))))
                {
                    await writer.Writer.WriteStartArrayAsync(); // begin all streams

                    await writer.WriteStream("roadnetwork", new object[] { new BeganRoadNetworkImport() });

                    foreach (var organization in organizations)
                    {
                        await writer.WriteStream(
                            "organization-" + organization.Code.ToLowerInvariant(), 
                            new object[] { organization });
                    }

                    await writer.WriteStream(
                        "roadnetwork", 
                        nodes
                            .Concat(segments)
                            .Concat(junctions)
                            .Concat(points)
                            .Concat(new [] { new CompletedRoadNetworkImport() })
                    );

                    await writer.Writer.WriteEndArrayAsync(); //end all streams

                    await writer.Writer.FlushAsync();

                    await entryStream.FlushAsync();
                }

                await fileStream.FlushAsync();
            }
        }
    }
}