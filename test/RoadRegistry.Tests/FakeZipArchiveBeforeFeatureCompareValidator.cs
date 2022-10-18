namespace RoadRegistry.Tests;

using System.IO.Compression;
using RoadRegistry.BackOffice.Uploads;

public class FakeZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveMetadata metadata)
    {
        return archive.GetEntry("error") != null
            ? ZipArchiveProblems.Single(new FileError("error", "reason"))
            : ZipArchiveProblems.None;
    }
}