namespace RoadRegistry.BackOffice.FeatureCompare;

public interface IZipArchiveFeatureCompareTranslatorFactory
{
    IZipArchiveFeatureCompareTranslator Create(string zipArchiveWriterVersion);
}

public class ZipArchiveFeatureCompareTranslatorFactory : IZipArchiveFeatureCompareTranslatorFactory
{
    private readonly FeatureCompare.V1.ZipArchiveFeatureCompareTranslator _v1;
    private readonly FeatureCompare.V2.ZipArchiveFeatureCompareTranslator _v2;

    public ZipArchiveFeatureCompareTranslatorFactory(
        FeatureCompare.V1.ZipArchiveFeatureCompareTranslator v1,
        FeatureCompare.V2.ZipArchiveFeatureCompareTranslator v2)
    {
        _v1 = v1;
        _v2 = v2;
    }

    public IZipArchiveFeatureCompareTranslator Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.V2)
        {
            return _v2;
        }

        return _v1;
    }
}
