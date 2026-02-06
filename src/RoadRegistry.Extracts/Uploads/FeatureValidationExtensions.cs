namespace RoadRegistry.Extracts.Uploads;

using System.IO.Compression;
using System.Text;
using Infrastructure.Extensions;

public static class FeatureValidationExtensions
{
    public static ZipArchiveProblems ValidateProjectionFileLambert72(this ZipArchive archive, FeatureType featureType, ExtractFileName fileName, Encoding encoding)
    {
        var prjFileName = featureType.ToProjectionFileName(fileName);
        var prjEntry = archive.FindEntry(prjFileName);
        if (prjEntry is null)
        {
            return ZipArchiveProblems.None
                .RequiredFileMissing(prjFileName);
        }

        return new ZipArchiveProjectionLambert72FormatEntryValidator(encoding)
            .Validate(prjEntry);
    }

    public static ZipArchiveProblems ValidateProjectionFileLambert08(this ZipArchive archive, FeatureType featureType, ExtractFileName fileName, Encoding encoding)
    {
        var prjFileName = featureType.ToProjectionFileName(fileName);
        var prjEntry = archive.FindEntry(prjFileName);
        if (prjEntry is null)
        {
            return ZipArchiveProblems.None
                .RequiredFileMissing(prjFileName);
        }

        return new ZipArchiveProjectionLambert08FormatEntryValidator(encoding)
            .Validate(prjEntry);
    }
}
