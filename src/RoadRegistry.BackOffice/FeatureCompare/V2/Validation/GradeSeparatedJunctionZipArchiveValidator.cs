namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class GradeSeparatedJunctionZipArchiveValidator : FeatureReaderZipArchiveValidator<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionZipArchiveValidator(GradeSeparatedJunctionFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.RltOgkruising, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
