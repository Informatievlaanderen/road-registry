namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;

    public interface IZipArchiveWriter
    {
        Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken);
    }
}
