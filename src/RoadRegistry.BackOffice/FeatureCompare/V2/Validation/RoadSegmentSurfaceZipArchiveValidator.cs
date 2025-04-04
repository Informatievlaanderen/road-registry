namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

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
