namespace RoadRegistry.BackOffice.FeatureCompare;

using Exceptions;
using FeatureToggles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
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

    public ZipArchiveFeatureCompareTranslator(Encoding encoding, ILogger logger,
        UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _logger = logger.ThrowIfNull();

        _translators = new IZipArchiveEntryFeatureCompareTranslator[]
        {
            new TransactionZoneFeatureCompareTranslator(encoding),
            new RoadNodeFeatureCompareTranslator(encoding),
            new RoadSegmentFeatureCompareTranslator(encoding),
            new RoadSegmentLaneFeatureCompareTranslator(encoding),
            new RoadSegmentWidthFeatureCompareTranslator(encoding),
            new RoadSegmentSurfaceFeatureCompareTranslator(encoding),
            new EuropeanRoadFeatureCompareTranslator(encoding),
            new NationalRoadFeatureCompareTranslator(encoding),
            new NumberedRoadFeatureCompareTranslator(encoding),
            new GradeSeparatedJunctionFeatureCompareTranslator(encoding, useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle)
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
