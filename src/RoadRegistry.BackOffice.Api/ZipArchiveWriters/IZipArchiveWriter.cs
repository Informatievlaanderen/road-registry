namespace RoadRegistry.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Schema;

    public interface IZipArchiveWriter
    {
        Task WriteAsync(ZipArchive archive, ShapeContext context, CancellationToken cancellationToken);
    }
}
