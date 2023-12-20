namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class EuropeanRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadZipArchiveValidator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttEuropweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
