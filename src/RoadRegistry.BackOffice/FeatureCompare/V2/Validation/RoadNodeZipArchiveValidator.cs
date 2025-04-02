namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Wegknoop, new[] { FeatureType.Extract, FeatureType.Change, FeatureType.Integration },
            featureReader)
    {
    }
}
