namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Wegknoop, new[] { FeatureType.Extract, FeatureType.Change, FeatureType.Integration },
            featureReader)
    {
    }
}
