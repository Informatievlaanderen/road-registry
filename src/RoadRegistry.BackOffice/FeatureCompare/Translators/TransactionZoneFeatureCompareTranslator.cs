namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase;
using RoadRegistry.BackOffice.Uploads;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class TransactionZoneFeatureCompareTranslator : FeatureCompareTranslatorBase<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var featureReader = new VersionedFeatureReader<Feature>(
            new ExtractsFeatureReader(Encoding)
        );

        var dbfFileName = GetDbfFileName(featureType, fileName);

        return featureReader.Read(entries, dbfFileName);
    }

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var features = ReadFeatures(FeatureType.Levering, entries, "TRANSACTIEZONES");
        var feature = features.SingleOrDefault();
        if (feature is not null)
        {
            changes = changes
                .WithReason(new Reason(feature.Attributes.BESCHRIJV))
                .WithOperatorName(string.IsNullOrEmpty(feature.Attributes.OPERATOR)
                    ? OperatorName.Unknown
                    : new OperatorName(feature.Attributes.OPERATOR))
                .WithOrganization(new OrganizationId(feature.Attributes.ORG));
        }

        return Task.FromResult(changes);
    }

    private class ExtractsFeatureReader : FeatureReader<TransactionZoneDbaseRecord, Feature>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, TransactionZoneDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, TransactionZoneDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new TransactionZoneFeatureCompareAttributes
            {
                BESCHRIJV = dbaseRecord.BESCHRIJV.Value,
                DOWNLOADID = dbaseRecord.DOWNLOADID.Value,
                OPERATOR = dbaseRecord.OPERATOR.Value,
                ORG = dbaseRecord.ORG.Value,
                TYPE = dbaseRecord.TYPE.Value
            });
        }
    }
}
