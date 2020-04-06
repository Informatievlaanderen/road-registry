namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Schema;

    public interface IZipArchiveWriter
    {
        Task WriteAsync(ZipArchive archive, BackOfficeContext context, CancellationToken cancellationToken);
    }
}
