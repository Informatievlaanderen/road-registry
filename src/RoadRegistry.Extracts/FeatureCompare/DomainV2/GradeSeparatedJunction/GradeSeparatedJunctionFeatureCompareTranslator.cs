namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schemas.DomainV2.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.Infrastructure;
using RoadSegment;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.RltOgkruising;
    private readonly ILogger _logger;

    public GradeSeparatedJunctionFeatureCompareTranslator(
        GradeSeparatedJunctionFeatureCompareFeatureReader featureReader,
        ILoggerFactory? loggerFactory = null)
        : base(featureReader)
    {
        _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var features = ReadExtractAndChangeFeatures(context.Archive, context);
        var problems = features.Problems;
        problems.ThrowIfError();

        var processedRecords = new List<Record>();

        var extractFeatures = features.Extract
            .Select(x => (
                Feature: x,
                LowerRoadSegmentId: context.MapToRoadSegmentId([FeatureType.Extract, FeatureType.Integration], x.Attributes.LowerRoadSegmentTempId),
                UpperRoadSegmentId: context.MapToRoadSegmentId([FeatureType.Extract, FeatureType.Integration], x.Attributes.UpperRoadSegmentTempId)))
            .ToList();

        foreach (var changeFeature in features.Change)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                .WithIdentifier(nameof(GradeSeparatedJunctionDbaseRecord.OK_OIDN), changeFeature.Attributes.Id);

            if (changeFeature.Attributes.LowerRoadSegmentTempId == changeFeature.Attributes.UpperRoadSegmentTempId)
            {
                problems += recordContext.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment(changeFeature.Attributes.LowerRoadSegmentTempId.ToInt32());
                continue;
            }

            var upperWegsegmentFeature = context.FindNotRemovedRoadSegmentByTempId([FeatureType.Change, FeatureType.Integration], changeFeature.Attributes.UpperRoadSegmentTempId);
            var lowerWegsegmentFeature = context.FindNotRemovedRoadSegmentByTempId([FeatureType.Change, FeatureType.Integration], changeFeature.Attributes.LowerRoadSegmentTempId);

            if (upperWegsegmentFeature is null || lowerWegsegmentFeature is null)
            {
                if (upperWegsegmentFeature is null)
                {
                    problems += recordContext.UpperRoadSegmentIdOutOfRange(changeFeature.Attributes.UpperRoadSegmentTempId.ToInt32());
                }

                if (lowerWegsegmentFeature is null)
                {
                    problems += recordContext.LowerRoadSegmentIdOutOfRange(changeFeature.Attributes.LowerRoadSegmentTempId.ToInt32());
                }

                continue;
            }

            var matchingExtractFeatures = extractFeatures
                .Where(x =>
                    x.UpperRoadSegmentId == upperWegsegmentFeature.RoadSegmentId
                    && x.LowerRoadSegmentId == lowerWegsegmentFeature.RoadSegmentId)
                .Select(x => x.Feature)
                .ToArray();
            if (!matchingExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added, lowerWegsegmentFeature.RoadSegmentId, upperWegsegmentFeature.RoadSegmentId));
                continue;
            }

            var matchingExtractFeaturesByType = matchingExtractFeatures.Where(x => x.Attributes.Type == changeFeature.Attributes.Type).ToArray();
            if (matchingExtractFeaturesByType.Any())
            {
                var identicalExtractFeature = matchingExtractFeaturesByType.FirstOrDefault(x => x.Attributes.Id == changeFeature.Attributes.Id)
                                              ?? matchingExtractFeaturesByType.First();

                processedRecords.Add(new Record(identicalExtractFeature, RecordType.Identical, lowerWegsegmentFeature.RoadSegmentId, upperWegsegmentFeature.RoadSegmentId));
                continue;
            }

            var matchingExtractFeature = matchingExtractFeatures.FirstOrDefault(x => x.Attributes.Id == changeFeature.Attributes.Id)
                                         ?? matchingExtractFeatures.First();

            processedRecords.Add(new Record(changeFeature, RecordType.Added, lowerWegsegmentFeature.RoadSegmentId, upperWegsegmentFeature.RoadSegmentId));
            processedRecords.Add(new Record(matchingExtractFeature, RecordType.Removed, default, default));
        }

        RemoveFeatures(GetFeaturesToRemove());

        // A crossing without an ongelijkgrondse kruising is not looked at here, unlike in the inwinning: filling in
        // traffic types is not part of what a GRB dienstverlener is asked to do, so the bijhouding neither refuses nor
        // flags it. See MissingGradeSeparatedJunction for the domain side.

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, processedRecords, cancellationToken);

        return Task.FromResult((changes, problems));

        List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> GetFeaturesToRemove()
        {
            var featuresToRemove = new List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>();
            var usedProcessedRecords = new List<Record>();

            foreach (var extractFeature in extractFeatures)
            {
                var matchingProcessedRecords = processedRecords
                    .Where(x => x.UpperRoadSegmentId == extractFeature.UpperRoadSegmentId
                                && x.LowerRoadSegmentId == extractFeature.LowerRoadSegmentId
                                && !usedProcessedRecords.Contains(x))
                    .ToList();

                if (!matchingProcessedRecords.Any())
                {
                    featuresToRemove.Add(extractFeature.Feature);
                }
                else
                {
                    usedProcessedRecords.AddRange(matchingProcessedRecords);
                }
            }

            return featuresToRemove;
        }

        void RemoveFeatures(ICollection<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> removeFeatures)
        {
            foreach (var feature in removeFeatures)
            {
                if (!processedRecords.Any(x => x.Feature.Attributes.Id == feature.Attributes.Id
                                               && x.RecordType.Equals(RecordType.Removed)))
                {
                    processedRecords.Add(new Record(feature, RecordType.Removed, default, default));
                }
            }
        }
    }

    private static TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    // A junction the delivery leaves untouched stays as it is: nothing to record.
                    break;
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = record.Feature.Attributes.Id,
                            LowerRoadSegmentId = record.LowerRoadSegmentId,
                            UpperRoadSegmentId = record.UpperRoadSegmentId,
                            Type = record.Feature.Attributes.Type
                        }
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = record.Feature.Attributes.Id
                        }
                    );
                    break;
            }
        }

        return changes;
    }

    private sealed record Record(Feature<GradeSeparatedJunctionFeatureCompareAttributes> Feature, RecordType RecordType, RoadSegmentId LowerRoadSegmentId, RoadSegmentId UpperRoadSegmentId);
}
