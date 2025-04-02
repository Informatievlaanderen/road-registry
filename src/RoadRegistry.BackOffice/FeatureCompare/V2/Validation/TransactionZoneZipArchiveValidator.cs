namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

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
