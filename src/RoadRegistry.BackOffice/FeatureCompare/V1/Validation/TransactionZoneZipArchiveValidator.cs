namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
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
