namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class NumberedRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadZipArchiveValidator(NumberedRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttGenumweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
