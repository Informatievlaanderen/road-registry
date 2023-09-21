namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;
using FeatureCompare;

public interface IZipArchiveValidator
{
    ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveValidatorContext context);
}

public class ZipArchiveValidatorContext : ZipArchiveFeatureReaderContext
{
    public ZipArchiveValidatorContext(ZipArchiveMetadata metadata)
        : base(metadata)
    {
    }
}
