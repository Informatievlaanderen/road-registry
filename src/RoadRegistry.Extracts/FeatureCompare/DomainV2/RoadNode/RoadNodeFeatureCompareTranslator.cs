namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNode.Changes;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class RoadNodeFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadNodeFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.Wegknoop;

    public RoadNodeFeatureCompareTranslator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base(featureReader)
    {
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        // load integration features only for validation purposes
        var (extractFeatures, changeFeatures, _, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, context);
        problems.ThrowIfError();

        if (changeFeatures.Any())
        {
            var batchCount = Debugger.IsAttached ? 1 : 4;

            var processedLeveringRecords = new ConcurrentDictionary<int, ICollection<RoadNodeFeatureCompareRecord>>();
            Parallel.Invoke(changeFeatures
                .SplitIntoBatches(batchCount)
                .Select((changeFeaturesBatch, index) => { return (Action)(() => { processedLeveringRecords.TryAdd(index, ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, context, cancellationToken)); }); })
                .ToArray());

            problems += AddProcessedRecordsToContext(processedLeveringRecords.OrderBy(x => x.Key).SelectMany(x => x.Value).ToList(), context, cancellationToken);
        }

        AddExtractRecordsToContext(extractFeatures, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context.RoadNodeRecords, context, cancellationToken);

        return Task.FromResult((changes, problems));
    }

    private List<RoadNodeFeatureCompareRecord> ProcessLeveringRecords(ICollection<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var clusterTolerance = 0.05; // cfr WVB in GRB

        var processedRecords = new List<RoadNodeFeatureCompareRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(clusterTolerance);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var identicalFeatures = intersectingGeometries.FindAll(extractFeature => AttributesEquals(extractFeature, changeFeature, context));
                if (identicalFeatures.Any())
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        identicalFeatures.First().Attributes.RoadNodeId,
                        RecordType.Identical));
                }
                else
                {
                    var extractFeature = intersectingGeometries.First();
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        context.ZipArchiveMetadata.Inwinning
                            ? changeFeature.Attributes
                            : changeFeature.Attributes.OnlyChangedAttributes(extractFeature.Attributes, extractFeature.Attributes.Geometry),
                        extractFeature.Attributes.RoadNodeId,
                        RecordType.Modified)
                    {
                        GeometryChanged = true
                    });
                }
            }
            else
            {
                var extractFeature = extractFeatures.FirstOrDefault(x => x.Attributes.RoadNodeId == changeFeature.Attributes.RoadNodeId);
                if (extractFeature is not null)
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        extractFeature.RecordNumber,
                        extractFeature.Attributes,
                        extractFeature.Attributes.RoadNodeId,
                        RecordType.Removed));
                }

                processedRecords.Add(new RoadNodeFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeature.Attributes,
                    changeFeature.Attributes.RoadNodeId,
                    RecordType.Added));
            }
        }

        return processedRecords;
    }

    private bool AttributesEquals(Feature<RoadNodeFeatureCompareAttributes> feature1, Feature<RoadNodeFeatureCompareAttributes> feature2, ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        return feature1.Attributes.Geometry.IsReasonablyEqualTo(feature2.Attributes.Geometry, context.Tolerances)
               && feature1.Attributes.Grensknoop == feature2.Attributes.Grensknoop;
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<RoadNodeFeatureCompareRecord> records, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadNodeChange
                        {
                            TemporaryId = record.Id,
                            OriginalId = record.Id != record.Attributes.RoadNodeId
                                ? record.Attributes.RoadNodeId
                                : null,
                            Geometry = record.Attributes.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = record.Attributes.Grensknoop!.Value
                        }
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = record.Id,
                            Geometry = record.GeometryChanged || context.ZipArchiveMetadata.Inwinning
                                ? record.Attributes.Geometry.ToRoadNodeGeometry()
                                : null,
                            Grensknoop = record.Attributes.Grensknoop
                        }
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadNodeChange
                        {
                            RoadNodeId = record.Id
                        }
                    );
                    break;
            }
        }

        return changes;
    }

    private ZipArchiveProblems AddProcessedRecordsToContext(ICollection<RoadNodeFeatureCompareRecord> processedRecords, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (record.RecordType != RecordType.Removed)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, record.RecordNumber)
                    .WithIdentifier(nameof(RoadNodeDbaseRecord.WK_OIDN), record.GetOriginalId());

                var existingRecords = context.RoadNodeRecords
                    .Where(x => x.RecordType != RecordType.Removed && x.GetActualId() == record.GetActualId())
                    .ToArray();

                if (existingRecords.Length > 1)
                {
                    problems += recordContext.IdentifierNotUnique(record.GetActualId(), record.RecordNumber);
                    continue;
                }

                var existingRecord = existingRecords.SingleOrDefault();
                if (existingRecord is not null)
                {
                    problems += recordContext.RoadNodeIsAlreadyProcessed(record.GetOriginalId(), existingRecord.GetOriginalId());
                    continue;
                }
            }

            context.RoadNodeRecords.Add(record);
        }

        return problems;
    }

    private void AddExtractRecordsToContext(List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadNode = context.RoadNodeRecords.Any(x => x.Id == extractFeature.Attributes.RoadNodeId
                                                                        && !x.RecordType.Equals(RecordType.Added));
            context.RoadNodeRecords.Add(new RoadNodeFeatureCompareRecord(FeatureType.Extract, extractFeature.RecordNumber, extractFeature.Attributes, extractFeature.Attributes.RoadNodeId,
                hasProcessedRoadNode ? RecordType.Identical : RecordType.Removed));
        }
    }
}
