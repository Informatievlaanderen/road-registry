namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class RoadSegmentWidthZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthZipArchiveValidator(RoadSegmentWidthFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegbreedte, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
