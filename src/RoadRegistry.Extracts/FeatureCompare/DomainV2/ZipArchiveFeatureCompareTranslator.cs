namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
using RoadRegistry.Extracts.Uploads;

public class ZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
{
    private readonly ILogger _logger;
    private readonly ICollection<IZipArchiveEntryFeatureCompareTranslator> _translators;

    public ZipArchiveFeatureCompareTranslator(
        TransactionZoneFeatureCompareTranslator transactionZoneTranslator,
        RoadNodeFeatureCompareTranslator roadNodeTranslator,
        RoadSegmentFeatureCompareTranslator roadSegmentTranslator,
        EuropeanRoadFeatureCompareTranslator europeanRoadTranslator,
        NationalRoadFeatureCompareTranslator nationalRoadTranslator,
        GradeSeparatedJunctionFeatureCompareTranslator gradeSeparatedJunctionTranslator,
        ILoggerFactory loggerFactory
    )
        : this([
                transactionZoneTranslator.ThrowIfNull(),
                roadNodeTranslator.ThrowIfNull(),
                roadSegmentTranslator.ThrowIfNull(),
                europeanRoadTranslator.ThrowIfNull(),
                nationalRoadTranslator.ThrowIfNull(),
                gradeSeparatedJunctionTranslator.ThrowIfNull()
            ],
            loggerFactory)
    {
    }

    private ZipArchiveFeatureCompareTranslator(
        ICollection<IZipArchiveEntryFeatureCompareTranslator> translators,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.ThrowIfNull().CreateLogger(GetType());
        _translators = translators.ThrowIfNull();
    }

    public static IZipArchiveFeatureCompareTranslator Create(
        ICollection<IZipArchiveEntryFeatureCompareTranslator> translators,
        ILoggerFactory loggerFactory)
    {
        return new ZipArchiveFeatureCompareTranslator(translators, loggerFactory);
    }

    public async Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);

        var changes = TranslatedChanges.Empty;

        var context = new ZipArchiveEntryFeatureCompareTranslateContext(archive, zipArchiveMetadata);

        foreach (var translator in _translators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var _ = _logger.TimeAction(translator.GetType().Name);

            (changes, var problems) = await translator.TranslateAsync(context, changes, cancellationToken);
            problems.ThrowIfError();
        }

        return changes;
    }
}
