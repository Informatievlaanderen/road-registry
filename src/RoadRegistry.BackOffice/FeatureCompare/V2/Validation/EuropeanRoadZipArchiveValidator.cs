namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class EuropeanRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadZipArchiveValidator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttEuropweg, [FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
