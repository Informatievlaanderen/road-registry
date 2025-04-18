namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

public interface IZipArchiveValidator
{
    Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken);
}
