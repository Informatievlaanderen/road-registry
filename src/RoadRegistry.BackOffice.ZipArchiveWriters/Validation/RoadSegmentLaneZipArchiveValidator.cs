namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadSegmentLaneZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentLaneFeatureCompareAttributes>
{
    public RoadSegmentLaneZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.AttRijstroken, new[] { FeatureType.Extract, FeatureType.Change },
            new RoadSegmentLaneFeatureCompareFeatureReader(encoding))
    {
    }
}
