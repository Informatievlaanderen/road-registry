namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using System.IO.Compression;
using BackOffice.ZipArchiveWriters.ExtractHost;
using Extracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;

public class ZipArchiveScenarioWithWriter<TContext> : ZipArchiveScenario where TContext : DbContext
{
    public ZipArchiveScenarioWithWriter(RecyclableMemoryStreamManager manager, IZipArchiveWriter<TContext> writer) : base(manager)
    {
        _writer = writer;
    }

    private TContext _context;
    private RoadNetworkExtractAssemblyRequest _request;
    private readonly IZipArchiveWriter<TContext> _writer;

    public ZipArchiveScenarioWithWriter<TContext> WithContext(TContext context)
    {
        _context = context;
        return this;
    }

    public ZipArchiveScenarioWithWriter<TContext> WithRequest(RoadNetworkExtractAssemblyRequest request)
    {
        _request = request;
        return this;
    }

    protected override Task Write(ZipArchive archive, CancellationToken ct)
    {
        return _writer.WriteAsync(archive, _request, _context, ct);
    }
}
