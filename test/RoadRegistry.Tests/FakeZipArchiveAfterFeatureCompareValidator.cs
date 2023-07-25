namespace RoadRegistry.Tests;

using System.IO.Compression;
using RoadRegistry.BackOffice.Uploads;

public class FakeZipArchiveAfterFeatureCompareValidator : IZipArchiveAfterFeatureCompareValidator
{
    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveValidatorContext context)
    {
        return archive.GetEntry("error") != null
            ? ZipArchiveProblems.Single(new FileError("error", "reason"))
            : ZipArchiveProblems.None;
    }
}