namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change, FeatureType.Integration],
            featureReader)
    {
    }
}
