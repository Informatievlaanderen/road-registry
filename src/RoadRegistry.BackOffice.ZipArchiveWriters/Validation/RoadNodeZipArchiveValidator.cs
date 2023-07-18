namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadNodeZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.Wegknoop, new[] { FeatureType.Integration, FeatureType.Extract, FeatureType.Change },
            new RoadNodeFeatureCompareFeatureReader(encoding))
    {
    }
}
