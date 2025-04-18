namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Extracts;
using Models;
using Readers;

public class EuropeanRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadZipArchiveValidator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
