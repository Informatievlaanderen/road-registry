namespace RoadRegistry.BackOffice.FeatureCompare;

using RoadRegistry.Extracts;

public interface IZipArchiveBeforeFeatureCompareValidatorFactory
{
    IZipArchiveBeforeFeatureCompareValidator Create(string zipArchiveWriterVersion);
}

public class ZipArchiveBeforeFeatureCompareValidatorFactory : IZipArchiveBeforeFeatureCompareValidatorFactory
{
    private readonly FeatureCompare.V1.Validation.ZipArchiveBeforeFeatureCompareValidator _validatorV1;
    private readonly FeatureCompare.V2.Validation.ZipArchiveBeforeFeatureCompareValidator _validatorV2;

    public ZipArchiveBeforeFeatureCompareValidatorFactory(
        FeatureCompare.V1.Validation.ZipArchiveBeforeFeatureCompareValidator validatorV1,
        FeatureCompare.V2.Validation.ZipArchiveBeforeFeatureCompareValidator validatorV2)
    {
        _validatorV1 = validatorV1;
        _validatorV2 = validatorV2;
    }

    public IZipArchiveBeforeFeatureCompareValidator Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV1_2)
        {
            return _validatorV2;
        }

        return _validatorV1;
    }
}
