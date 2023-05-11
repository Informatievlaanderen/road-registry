namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase;

public class TransactionZoneFeatureCompareFeatureReader : VersionedFeatureReader<Feature<TransactionZoneFeatureCompareAttributes>>
{
    public TransactionZoneFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<TransactionZoneDbaseRecord, Feature<TransactionZoneFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, TransactionZoneDbaseRecord.Schema)
        {
        }

        protected override Feature<TransactionZoneFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, TransactionZoneDbaseRecord dbaseRecord)
        {
            return new Feature<TransactionZoneFeatureCompareAttributes>(recordNumber, new TransactionZoneFeatureCompareAttributes
            {
                Description = dbaseRecord.BESCHRIJV.Value,
                DownloadId = dbaseRecord.DOWNLOADID.Value,
                OperatorName = dbaseRecord.OPERATOR.Value,
                Organization = dbaseRecord.ORG.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }
}
