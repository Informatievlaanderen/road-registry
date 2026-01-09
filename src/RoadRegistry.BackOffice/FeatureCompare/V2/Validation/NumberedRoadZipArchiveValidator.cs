namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Extracts;
using Models;
using Readers;
using RoadRegistry.Extracts;

public class NumberedRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadZipArchiveValidator(NumberedRoadFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
