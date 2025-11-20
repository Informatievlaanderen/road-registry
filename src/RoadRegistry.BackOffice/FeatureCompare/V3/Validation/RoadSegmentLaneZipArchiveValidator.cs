namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class RoadSegmentLaneZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentLaneFeatureCompareAttributes>
{
    public RoadSegmentLaneZipArchiveValidator(RoadSegmentLaneFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
