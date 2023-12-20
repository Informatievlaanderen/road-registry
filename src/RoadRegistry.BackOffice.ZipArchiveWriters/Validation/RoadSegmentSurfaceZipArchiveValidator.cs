namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadSegmentSurfaceZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceZipArchiveValidator(RoadSegmentSurfaceFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegverharding, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
