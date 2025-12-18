namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

using RoadRegistry.Extracts;

public interface IBeforeFeatureCompareZipArchiveCleanerFactory
{
    IBeforeFeatureCompareZipArchiveCleaner Create(string zipArchiveWriterVersion);
}

public class BeforeFeatureCompareZipArchiveCleanerFactory : IBeforeFeatureCompareZipArchiveCleanerFactory
{
    private readonly V1.BeforeFeatureCompareZipArchiveCleaner _v1;
    private readonly V2.BeforeFeatureCompareZipArchiveCleaner _v2;

    public BeforeFeatureCompareZipArchiveCleanerFactory(FileEncoding fileEncoding)
    {
        _v1 = new V1.BeforeFeatureCompareZipArchiveCleaner(fileEncoding);
        _v2 = new V2.BeforeFeatureCompareZipArchiveCleaner(fileEncoding);
    }

    public IBeforeFeatureCompareZipArchiveCleaner Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.V2)
        {
            return _v2;
        }

        return _v1;
    }
}
