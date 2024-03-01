namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttNationweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
