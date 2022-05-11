namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public interface IZipArchiveWriter<in TContext> where TContext : DbContext
    {
        Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken);
    }
}
