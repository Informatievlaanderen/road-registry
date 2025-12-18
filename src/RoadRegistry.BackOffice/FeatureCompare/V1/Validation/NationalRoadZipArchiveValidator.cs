namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using Translators;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttNationweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
