namespace RoadRegistry.BackOffice.FeatureCompare.V3.Validation;

using Extracts;
using Models;
using Readers;

public class TransactionZoneZipArchiveValidator : FeatureReaderZipArchiveValidator<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneZipArchiveValidator(TransactionZoneFeatureCompareFeatureReader featureReader)
        : base([FeatureType.Change], featureReader)
    {
    }
}
