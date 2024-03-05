namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class RoadSegmentSurfaceZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceZipArchiveValidator(RoadSegmentSurfaceFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegverharding, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
