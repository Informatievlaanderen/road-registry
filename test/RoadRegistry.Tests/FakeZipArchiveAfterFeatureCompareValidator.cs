namespace RoadRegistry.Tests;

using System.IO.Compression;
using RoadRegistry.BackOffice.Uploads;

public class FakeZipArchiveAfterFeatureCompareValidator : IZipArchiveAfterFeatureCompareValidator
{
    public async Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        return archive.GetEntry("error") != null
            ? ZipArchiveProblems.Single(new FileError("error", "reason"))
            : ZipArchiveProblems.None;
    }
}
