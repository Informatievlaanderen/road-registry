namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class GradeSeparatedJunctionZipArchiveValidator : FeatureReaderZipArchiveValidator<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionZipArchiveValidator(GradeSeparatedJunctionFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.RltOgkruising, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
