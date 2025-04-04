namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

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
