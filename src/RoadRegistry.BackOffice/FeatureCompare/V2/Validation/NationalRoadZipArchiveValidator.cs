namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttNationweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
