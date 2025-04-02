namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class EuropeanRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadZipArchiveValidator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttEuropweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
