namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Extracts;
using Models;
using Readers;
using RoadRegistry.Extracts;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change, FeatureType.Integration],
            featureReader)
    {
    }
}
