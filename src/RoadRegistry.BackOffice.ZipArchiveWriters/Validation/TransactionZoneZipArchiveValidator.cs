namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class TransactionZoneZipArchiveValidator : FeatureReaderZipArchiveValidator<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.Transactiezones, new[] { FeatureType.Change },
            new TransactionZoneFeatureCompareFeatureReader(encoding))
    {
    }
}
