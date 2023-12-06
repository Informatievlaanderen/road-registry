namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class GradeSeparatedJunctionZipArchiveValidator : FeatureReaderZipArchiveValidator<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionZipArchiveValidator(GradeSeparatedJunctionFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.RltOgkruising, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
