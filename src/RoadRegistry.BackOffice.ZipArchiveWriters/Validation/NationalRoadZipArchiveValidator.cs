namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttNationweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
