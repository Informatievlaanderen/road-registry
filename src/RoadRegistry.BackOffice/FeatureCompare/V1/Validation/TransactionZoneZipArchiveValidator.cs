namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using Readers;
using RoadRegistry.BackOffice.Extracts;
using Translators;

public class TransactionZoneZipArchiveValidator : FeatureReaderZipArchiveValidator<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneZipArchiveValidator(TransactionZoneFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Transactiezones,
            [FeatureType.Change],
            featureReader)
    {
    }
}
