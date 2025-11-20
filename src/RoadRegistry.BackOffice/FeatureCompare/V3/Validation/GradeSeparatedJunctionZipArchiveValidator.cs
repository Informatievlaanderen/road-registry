namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class GradeSeparatedJunctionZipArchiveValidator : FeatureReaderZipArchiveValidator<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionZipArchiveValidator(GradeSeparatedJunctionFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
