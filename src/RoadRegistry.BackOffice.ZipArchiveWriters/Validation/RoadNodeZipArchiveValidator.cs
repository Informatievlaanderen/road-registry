namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Wegknoop, new[] { FeatureType.Extract, FeatureType.Change, FeatureType.Integration },
            featureReader)
    {
    }
}
