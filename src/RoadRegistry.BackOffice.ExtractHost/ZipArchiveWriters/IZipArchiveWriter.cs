namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Extracts;
    using Microsoft.EntityFrameworkCore;

    public interface IZipArchiveWriter<in TContext> where TContext : DbContext
    {
        Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
            CancellationToken cancellationToken);
    }
}
