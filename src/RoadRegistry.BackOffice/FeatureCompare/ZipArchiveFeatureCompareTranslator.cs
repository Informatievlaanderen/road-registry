namespace RoadRegistry.BackOffice.FeatureCompare;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Translators;
using Uploads;

public interface IZipArchiveFeatureCompareTranslator : IZipArchiveTranslatorAsync
{
}

public class ZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
{
    private readonly ILogger _logger;
    private readonly IReadOnlyCollection<IZipArchiveEntryFeatureCompareTranslator> _translators;

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
        ILogger<ZipArchiveFeatureCompareTranslator> logger
    )
    {
        _logger = logger.ThrowIfNull();

        _translators = new IZipArchiveEntryFeatureCompareTranslator[]
        {
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
        };
    }

    public async Task<TranslatedChanges> Translate(ZipArchive archive, CancellationToken cancellationToken)
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
