namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class TransactionZoneZipArchiveValidator : FeatureReaderZipArchiveValidator<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneZipArchiveValidator(ITransactionZoneFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Transactiezones,
            [FeatureType.Change],
            featureReader)
    {
    }
}
