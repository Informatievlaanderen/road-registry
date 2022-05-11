namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IO;

    public abstract class ZipArchiveScenario
    {
        private readonly RecyclableMemoryStreamManager _manager;

        protected ZipArchiveScenario(RecyclableMemoryStreamManager manager)
        {
            _manager = manager;
        }

        protected abstract Task Write(ZipArchive archive, CancellationToken ct);

        public async Task Assert(Action<ZipArchive> assert)
        {
            using (var memoryStream = _manager.GetStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    await Write(archive, CancellationToken.None);
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
