namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Readers;
using Extracts;
using Translators;

public class EuropeanRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadZipArchiveValidator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttEuropweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
