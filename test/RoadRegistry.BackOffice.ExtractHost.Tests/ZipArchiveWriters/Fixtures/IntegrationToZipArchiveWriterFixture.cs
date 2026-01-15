namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters.Fixtures;

using System.Diagnostics;
using System.Text;
using Abstractions;
using BackOffice.ZipArchiveWriters.ExtractHost.V1;
using Editor.Schema;
using Extracts;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Infrastructure;
using ZipArchiveWriterFactory = BackOffice.ZipArchiveWriters.ExtractHost.ZipArchiveWriterFactory;

public class IntegrationToZipArchiveWriterFixture : ZipArchiveWriterFixture, IAsyncLifetime
{
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;

    public IntegrationToZipArchiveWriterFixture(WKTReader wktReader, RecyclableMemoryStreamManager memoryStreamManager, Func<EditorContext> contextFactory, IStreetNameCache streetNameCache)
        : base(wktReader)
    {
        var writer = new IntegrationToZipArchiveWriter(new ZipArchiveWriterOptions(), streetNameCache, memoryStreamManager, Encoding.UTF8);
        _assembler = new RoadNetworkExtractArchiveAssembler(
            memoryStreamManager,
            contextFactory,
            new ZipArchiveWriterFactory(writer, writer));
    }

    public TimeSpan ElapsedTimeSpan { get; private set; }
    public override FileInfo FileInfo => new(Path.Combine("ZipArchiveWriters", "Fixtures", "RoadNodesToZipArchiveWriterFixture.wkt"));

    public override RoadNetworkExtractAssemblyRequest Request => new(
        new DownloadId(),
        new ExtractDescription("TEST"),
        (IPolygonal)Result.Single(),
        isInformative: false,
        zipArchiveWriterVersion: null);

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
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
}
