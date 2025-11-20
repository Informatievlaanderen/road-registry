namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class RoadSegmentSurfaceZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceZipArchiveValidator(RoadSegmentSurfaceFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
