namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using Translators;

public class TransactionZoneZipArchiveValidator : FeatureReaderZipArchiveValidator<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneZipArchiveValidator(TransactionZoneFeatureCompareFeatureReader featureReader)
        : base(ExtractFileName.Transactiezones, new[] { FeatureType.Change },
            featureReader)
    {
    }
}
