namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;

public abstract class VersionedZipArchiveCleaner : IZipArchiveCleaner
{
    private readonly IZipArchiveCleaner[] _cleaners;

    protected VersionedZipArchiveCleaner(params IZipArchiveCleaner[] cleaners)
    {
        _cleaners = cleaners;
    }

    public async Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        foreach (var cleaner in _cleaners)
        {
            var result = await cleaner.CleanAsync(archive, cancellationToken);
            if (result != CleanResult.NotApplicable)
            {
                return result;
            }
        }

        return CleanResult.NoChanges;
    }
}
