namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;

    public interface IZipArchiveWriter<in TContext> where TContext : DbContext
    {
        Task WriteAsync(ZipArchive archive, MultiPolygon contour, TContext context, CancellationToken cancellationToken);
    }
}
