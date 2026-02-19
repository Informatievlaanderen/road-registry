namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts.Uploads;
using TranslatedChanges = TranslatedChanges;

public class TransactionZoneFeatureCompareTranslator : FeatureCompareTranslatorBase<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneFeatureCompareTranslator(TransactionZoneFeatureCompareFeatureReader featureReader)
        : base(featureReader)
    {
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (features, problems) = ReadFeatures(context.Archive, FeatureType.Change, context);
        problems.ThrowIfError();

        context.TransactionZone = features.Single().Attributes;

        return Task.FromResult((changes, problems));
    }
}
