namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

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
