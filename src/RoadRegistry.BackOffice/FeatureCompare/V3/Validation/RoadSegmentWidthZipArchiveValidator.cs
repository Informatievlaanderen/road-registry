namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class RoadSegmentWidthZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthZipArchiveValidator(RoadSegmentWidthFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
