namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

internal class TransactionZoneFeatureCompareTranslator : FeatureCompareTranslatorBase<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override List<Feature<TransactionZoneFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new TransactionZoneFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var features = ReadFeatures(entries, FeatureType.Levering, "TRANSACTIEZONES");
        var feature = features.SingleOrDefault();
        if (feature is not null)
        {
            changes = changes
                .WithReason(new Reason(feature.Attributes.Description))
                .WithOperatorName(string.IsNullOrEmpty(feature.Attributes.OperatorName)
                    ? OperatorName.Unknown
                    : new OperatorName(feature.Attributes.OperatorName))
                .WithOrganization(new OrganizationId(feature.Attributes.Organization));
        }

        return Task.FromResult(changes);
    }
}
