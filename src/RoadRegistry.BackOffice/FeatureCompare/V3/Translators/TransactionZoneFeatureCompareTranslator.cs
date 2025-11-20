namespace RoadRegistry.BackOffice.FeatureCompare.V3.Translators;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

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

        var feature = features.Single();

        changes = changes
            .WithReason(new Reason(feature.Attributes.Description))
            .WithOperatorName(string.IsNullOrEmpty(feature.Attributes.OperatorName)
                ? OperatorName.Unknown
                : new OperatorName(feature.Attributes.OperatorName))
            .WithOrganization(new OrganizationId(feature.Attributes.Organization));

        return Task.FromResult((changes, problems));
    }
}
