namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;
using RoadRegistry.BackOffice.Uploads;

public class ZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
{
    private readonly ILogger _logger;
    private readonly ICollection<IZipArchiveEntryFeatureCompareTranslator> _translators;

    public ZipArchiveFeatureCompareTranslator(
        TransactionZoneFeatureCompareTranslator transactionZoneTranslator,
        RoadNodeFeatureCompareTranslator roadNodeTranslator,
        RoadSegmentFeatureCompareTranslator roadSegmentTranslator,
        RoadSegmentLaneFeatureCompareTranslator roadSegmentLaneTranslator,
        RoadSegmentWidthFeatureCompareTranslator roadSegmentWidthTranslator,
        RoadSegmentSurfaceFeatureCompareTranslator roadSegmentSurfaceTranslator,
        EuropeanRoadFeatureCompareTranslator europeanRoadTranslator,
        NationalRoadFeatureCompareTranslator nationalRoadTranslator,
        NumberedRoadFeatureCompareTranslator numberedRoadTranslator,
        GradeSeparatedJunctionFeatureCompareTranslator gradeSeparatedJunctionTranslator,
        ILoggerFactory loggerFactory
    )
        : this([
                transactionZoneTranslator.ThrowIfNull(),
                roadNodeTranslator.ThrowIfNull(),
                roadSegmentTranslator.ThrowIfNull(),
                roadSegmentLaneTranslator.ThrowIfNull(),
                roadSegmentWidthTranslator.ThrowIfNull(),
                roadSegmentSurfaceTranslator.ThrowIfNull(),
                europeanRoadTranslator.ThrowIfNull(),
                nationalRoadTranslator.ThrowIfNull(),
                numberedRoadTranslator.ThrowIfNull(),
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

    public async Task<TranslatedChanges> TranslateAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);

        var changes = TranslatedChanges.Empty;

        var context = new ZipArchiveEntryFeatureCompareTranslateContext(archive, ZipArchiveMetadata.Empty);

        foreach (var translator in _translators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sw = Stopwatch.StartNew();
            _logger.LogInformation("{Type} started...", translator.GetType().Name);

            (changes, var problems) = await translator.TranslateAsync(context, changes, cancellationToken);
            problems.ThrowIfError();

            _logger.LogInformation("{Type} completed in {Elapsed}", translator.GetType().Name, sw.Elapsed);
        }

        return changes;
    }
}
