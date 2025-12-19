namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using Extracts;
using Models;
using Readers;
using RoadRegistry.Extracts;

public class NationalRoadZipArchiveValidator : FeatureReaderZipArchiveValidator<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadZipArchiveValidator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Extract, FeatureType.Change],
            featureReader)
    {
    }
}
