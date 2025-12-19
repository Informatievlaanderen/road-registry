namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using Translators;

public class RoadSegmentSurfaceZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceZipArchiveValidator(RoadSegmentSurfaceFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttWegverharding, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
