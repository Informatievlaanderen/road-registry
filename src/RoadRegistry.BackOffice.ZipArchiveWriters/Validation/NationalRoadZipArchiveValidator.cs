namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.AttNationweg, new[] { FeatureType.Extract, FeatureType.Change },
            new NationalRoadFeatureCompareFeatureReader(encoding))
    {
    }
}
