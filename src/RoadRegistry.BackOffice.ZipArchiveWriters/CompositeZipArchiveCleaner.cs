namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;

public abstract class CompositeZipArchiveCleaner : IZipArchiveCleaner
{
    private readonly IZipArchiveCleaner[] _cleaners;

    protected CompositeZipArchiveCleaner(params IZipArchiveCleaner[] cleaners)
    {
        _cleaners = cleaners;
    }

    public async Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        var results = new List<CleanResult>();

        foreach (var cleaner in _cleaners)
        {
            results.Add(await cleaner.CleanAsync(archive, cancellationToken));
        }

        return results.Any(x => x == CleanResult.Changed)
            ? CleanResult.Changed
            : CleanResult.NoChanges;
    }
}
