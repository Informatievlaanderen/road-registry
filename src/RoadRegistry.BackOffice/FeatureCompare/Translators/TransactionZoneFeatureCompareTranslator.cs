namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Uploads;

internal class TransactionZoneFeatureCompareTranslator : FeatureCompareTranslatorBase<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override (List<Feature<TransactionZoneFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var featureReader = new TransactionZoneFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(archive, featureType, fileName, context);
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (features, problems) = ReadFeatures(context.Archive, FeatureType.Change, ExtractFileName.Transactiezones, context);
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

        return Task.FromResult((changes, problems));
    }
}
