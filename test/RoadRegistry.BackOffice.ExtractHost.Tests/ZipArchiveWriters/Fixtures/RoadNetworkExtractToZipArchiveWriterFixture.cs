namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters.Fixtures;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Abstractions;
using Azure.Core;
using BackOffice.ZipArchiveWriters.ExtractHost;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Extracts;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class RoadNetworkExtractToZipArchiveWriterFixture : ZipArchiveWriterFixture, IAsyncLifetime
{
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;

    public override FileInfo FileInfo => new(Path.Combine("ZipArchiveWriters", "Fixtures", "RoadNodesToZipArchiveWriterFixture.wkt"));

    public override RoadNetworkExtractAssemblyRequest Request => new(
        new ExternalExtractRequestId("TEST"),
        new DownloadId(),
        new ExtractDescription("TEST"),
        (IPolygonal)Result.Single());

    public RoadNetworkExtractToZipArchiveWriterFixture(
        WKTReader wktReader,
        RecyclableMemoryStreamManager memoryStreamManager,
        Func<EditorContext> contextFactory,
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache)
        : base(wktReader)
    {
        var zipArchiveWriter = new RoadNetworkExtractToZipArchiveWriter(
            zipArchiveWriterOptions,
            streetNameCache,
            memoryStreamManager,
            Encoding.UTF8);

        _assembler = new RoadNetworkExtractArchiveAssembler(
            memoryStreamManager,
            contextFactory,
            zipArchiveWriter);
    }

    public async Task InitializeAsync()
    {
        var sw = new Stopwatch();
        sw.Start();
        using (var content = await _assembler.AssembleArchive(Request, CancellationToken.None))
        {
            sw.Stop();

            ElapsedTimeSpan = sw.Elapsed;
            content.Position = 0L;
            var fileBytes = content.ToArray();

            File.WriteAllBytes(Path.ChangeExtension(FileInfo.FullName, ".zip"), fileBytes);
        }
    }

    public TimeSpan ElapsedTimeSpan { get; private set; }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
