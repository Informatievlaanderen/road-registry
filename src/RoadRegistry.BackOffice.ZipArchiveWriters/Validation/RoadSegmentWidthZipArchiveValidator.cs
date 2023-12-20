namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadSegmentWidthZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthZipArchiveValidator(RoadSegmentWidthFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegbreedte, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
