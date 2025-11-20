namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class NumberedRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadZipArchiveValidator(NumberedRoadFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
