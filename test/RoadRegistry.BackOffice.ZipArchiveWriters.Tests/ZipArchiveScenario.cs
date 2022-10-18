namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests;

using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;

public class ZipArchiveScenario<TContext> where TContext : DbContext
{
    public ZipArchiveScenario(RecyclableMemoryStreamManager manager, IZipArchiveWriter<TContext> writer)
    {
        _manager = manager;
        _writer = writer;
    }

    private TContext _context;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IZipArchiveWriter<TContext> _writer;

    public async Task Assert(Action<ZipArchive> assert)
    {
        using (var memoryStream = _manager.GetStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                await _writer.WriteAsync(archive, _context, CancellationToken.None);
            }

            memoryStream.Position = 0;
            using (var readArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false, Encoding.UTF8))
            {
                assert(readArchive);
            }
        }
    }

    public ZipArchiveScenario<TContext> WithContext(TContext context)
    {
        _context = context;
        return this;
    }
}
