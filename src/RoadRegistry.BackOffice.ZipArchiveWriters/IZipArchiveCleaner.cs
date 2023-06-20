namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

public interface IZipArchiveCleaner
{
    Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken);
}

public enum CleanResult
{
    NoChanges,
    NotApplicable,
    Changed
}
