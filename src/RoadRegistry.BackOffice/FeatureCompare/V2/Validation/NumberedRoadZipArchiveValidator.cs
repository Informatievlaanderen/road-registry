namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class NumberedRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadZipArchiveValidator(NumberedRoadFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttGenumweg, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
