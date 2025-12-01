namespace RoadRegistry.BackOffice.FeatureCompare.V3.TransactionZone;

using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Uploads;
using TranslatedChanges = TranslatedChanges;

public class TransactionZoneFeatureCompareTranslator : FeatureCompareTranslatorBase<TransactionZoneFeatureCompareAttributes>
{
    public TransactionZoneFeatureCompareTranslator(TransactionZoneFeatureCompareFeatureReader featureReader)
        : base(featureReader)
    {
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (_, problems) = ReadFeatures(context.Archive, FeatureType.Change, context);

        problems.ThrowIfError();

        return Task.FromResult((changes, problems));
    }
}
