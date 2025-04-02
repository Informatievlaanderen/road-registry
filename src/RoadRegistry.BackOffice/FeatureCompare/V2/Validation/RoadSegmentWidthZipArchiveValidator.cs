namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class RoadSegmentWidthZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthZipArchiveValidator(RoadSegmentWidthFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegbreedte, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
