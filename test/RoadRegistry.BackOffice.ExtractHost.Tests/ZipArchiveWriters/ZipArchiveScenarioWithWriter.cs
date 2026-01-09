namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using System.IO.Compression;
using BackOffice.ZipArchiveWriters.ExtractHost;
using Editor.Schema;
using Extracts;
using Microsoft.IO;
using RoadRegistry.Extracts;

public class ZipArchiveScenarioWithWriter : ZipArchiveScenario
{
    private readonly IZipArchiveWriter _writer;
    private EditorContext _context;
    private RoadNetworkExtractAssemblyRequest _request;

    public ZipArchiveScenarioWithWriter(RecyclableMemoryStreamManager manager, IZipArchiveWriter writer) : base(manager)
    {
        _writer = writer;
    }

    public ZipArchiveScenarioWithWriter WithContext(EditorContext context)
    {
        _context = context;
        return this;
    }

    public ZipArchiveScenarioWithWriter WithRequest(RoadNetworkExtractAssemblyRequest request)
    {
        _request = request;
        return this;
    }

    protected override Task Write(ZipArchive archive, CancellationToken ct)
    {
        return _writer.WriteAsync(archive, _request, new ZipArchiveDataProvider(_context), ct);
    }
}
