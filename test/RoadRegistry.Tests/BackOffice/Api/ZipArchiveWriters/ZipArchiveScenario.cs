namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using RoadRegistry.Framework.Containers;

    public class ZipArchiveScenario<TContext> where TContext : DbContext
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly IZipArchiveWriter<TContext> _writer;
        private TContext _context;

        public ZipArchiveScenario(RecyclableMemoryStreamManager manager, IZipArchiveWriter<TContext> writer)
        {
            _manager = manager;
            _writer = writer;
        }

        public ZipArchiveScenario<TContext> WithContext(TContext context)
        {
            _context = context;
            return this;
        }

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
    }
}
