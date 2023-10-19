namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadSegmentZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentFeatureCompareAttributes>
{
    public RoadSegmentZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.Wegsegment, new[] { FeatureType.Integration, FeatureType.Extract, FeatureType.Change },
            new RoadSegmentFeatureCompareFeatureReader(encoding))
    {
    }
}
