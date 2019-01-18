namespace RoadRegistry.Api.Downloads
{
    using System;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using BackOffice.Schema;

    public class RoadNodeArchiveWriter
    {
        public Task WriteAsync(ZipArchive archive, ShapeContext context)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Task.CompletedTask;
        }
    }
}
