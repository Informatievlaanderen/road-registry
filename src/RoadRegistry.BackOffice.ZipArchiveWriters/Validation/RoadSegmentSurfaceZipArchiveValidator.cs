namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class RoadSegmentSurfaceZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.AttWegverharding, new[] { FeatureType.Extract, FeatureType.Change },
            new RoadSegmentSurfaceFeatureCompareFeatureReader(encoding))
    {
    }
}
