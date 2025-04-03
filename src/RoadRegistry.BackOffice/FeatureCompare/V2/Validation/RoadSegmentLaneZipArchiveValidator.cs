namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class RoadSegmentLaneZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentLaneFeatureCompareAttributes>
{
    public RoadSegmentLaneZipArchiveValidator(RoadSegmentLaneFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.AttRijstroken, new[] { FeatureType.Extract, FeatureType.Change },
            featureReader)
    {
    }
}
