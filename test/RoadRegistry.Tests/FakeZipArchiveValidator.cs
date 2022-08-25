namespace RoadRegistry;

using System.IO.Compression;
using BackOffice.Uploads;

public class FakeZipArchiveValidator : IZipArchiveValidator
{
    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveMetadata metadata)
    {
        return archive.GetEntry("error") != null
            ? ZipArchiveProblems.Single(new FileError("error", "reason"))
            : ZipArchiveProblems.None;
    }
}
