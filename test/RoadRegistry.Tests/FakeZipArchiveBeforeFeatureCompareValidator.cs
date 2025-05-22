namespace RoadRegistry.Tests;

using System.IO.Compression;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.Uploads;

public class FakeZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    public Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
        return Task.FromResult(archive.GetEntry("error") != null
            ? ZipArchiveProblems.Single(new FileError("error", "reason"))
            : ZipArchiveProblems.None);
    }
}
