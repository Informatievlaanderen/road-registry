namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Index.Strtree;
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
            var processedLeveringRecords = ProcessLeveringRecordsInParallel(
                changeFeatures, extractFeatures, context, cancellationToken);

            problems += AddProcessedRecordsToContext(processedLeveringRecords, context, cancellationToken);
        }

        AddExtractRecordsToContext(extractFeatures, context, cancellationToken);
        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context, cancellationToken);

        return Task.FromResult((changes, problems));
    }

    private List<RoadNodeFeatureCompareRecord> ProcessLeveringRecordsInParallel(
        List<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures,
        List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var batchCount = Debugger.IsAttached ? 1 : Math.Max(2, Environment.ProcessorCount);

        var spatialIndex = new STRtree<Feature<RoadNodeFeatureCompareAttributes>>();
        foreach (var feature in extractFeatures)
        {
            spatialIndex.Insert(feature.Attributes.Geometry.EnvelopeInternal, feature);
        }

        spatialIndex.Build();

        var extractFeaturesDictionary = extractFeatures
            .ToDictionary(x => x.Attributes.RoadNodeId, x => x);

        var processedLeveringRecords = new ConcurrentDictionary<int, List<RoadNodeFeatureCompareRecord>>();
        Parallel.Invoke(changeFeatures
            .SplitIntoBatches(batchCount)
            .Select((changeFeaturesBatch, index) =>
            {
                return (Action)(() =>
                {
                    processedLeveringRecords.TryAdd(index,
                        ProcessLeveringRecords(changeFeaturesBatch, extractFeaturesDictionary, spatialIndex, context, cancellationToken));
                });
            })
            .ToArray());

        var processedLeveringRecordsList = processedLeveringRecords
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value)
            .ToList();
        return processedLeveringRecordsList;
    }

    private List<RoadNodeFeatureCompareRecord> ProcessLeveringRecords(
        ICollection<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures,
        IDictionary<RoadNodeId, Feature<RoadNodeFeatureCompareAttributes>> extractFeatures,
        STRtree<Feature<RoadNodeFeatureCompareAttributes>> extractFeaturesSpatialIndex,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var clusterTolerance = VerificationContextTolerances.RoadNodeBuffer.GeometryTolerance;

        var processedRecords = new List<RoadNodeFeatureCompareRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var geometryEnvelope = changeFeature.Attributes.Geometry.EnvelopeInternal.Copy();
            geometryEnvelope.ExpandBy(clusterTolerance);

            var intersectingGeometries = extractFeaturesSpatialIndex
                .Query(geometryEnvelope)
                .Where(x => x.Attributes.Geometry.IsWithinDistance(changeFeature.Attributes.Geometry, clusterTolerance))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var identicalFeatures = intersectingGeometries.FindAll(extractFeature => AttributesEquals(extractFeature, changeFeature, context));
                if (identicalFeatures.Any())
                {
                    var extractFeature = identicalFeatures.First();
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        extractFeature.Attributes.RoadNodeId,
                        RecordType.Identical));
                    var isTemporarySchijnknoop = extractFeature.Attributes.RoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractFeature.Attributes.RoadNodeId, 0);
                    }
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
                    var isTemporarySchijnknoop = extractFeature.Attributes.RoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractFeature.Attributes.RoadNodeId, 0);
                    }
                }
            }
            else
            {
                if (extractFeatures.TryGetValue(changeFeature.Attributes.RoadNodeId, out var extractFeature))
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        extractFeature.RecordNumber,
                        extractFeature.Attributes,
                        extractFeature.Attributes.RoadNodeId,
                        RecordType.Removed));
                    var isTemporarySchijnknoop = extractFeature.Attributes.RoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractFeature.Attributes.RoadNodeId, 0);
                    }
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

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var roadNodeFeatureCompareRecords = context.GetRoadNodeRecords(FeatureType.Change);
        foreach (var record in roadNodeFeatureCompareRecords)
        {
            var recordType = record.RecordType;

            var isTemporarySchijnknoop = record.Id >= RoadNodeConstants.InitialTemporarySchijnknoopId;
            if (isTemporarySchijnknoop)
            {
                if (recordType == RecordType.Identical || recordType == RecordType.Modified)
                {
                    recordType = RecordType.Added;
                }

                if (recordType == RecordType.Removed)
                {
                    continue;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            switch (recordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    if (context.ZipArchiveMetadata.Inwinning)
                    {
                        changes = changes.AppendChange(
                            new ModifyRoadNodeChange
                            {
                                RoadNodeId = record.Id,
                                Geometry = record.Attributes.Geometry.ToRoadNodeGeometry(),
                                Grensknoop = record.Attributes.Grensknoop
                            }
                        );
                    }
                    break;
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
        var seenByActualId = new Dictionary<RoadNodeId, RoadNodeFeatureCompareRecord>();
        var toAdd = new List<RoadNodeFeatureCompareRecord>(processedRecords.Count);

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (record.RecordType != RecordType.Removed)
            {
                var actualId = record.GetActualId();
                if (seenByActualId.TryGetValue(actualId, out var existing))
                {
                    var recordContext = FileName
                        .AtDbaseRecord(FeatureType.Change, record.RecordNumber)
                        .WithIdentifier(nameof(RoadNodeDbaseRecord.WK_OIDN), record.GetOriginalId());
                    problems += recordContext.RoadNodeIsAlreadyProcessed(record.GetOriginalId(), existing.GetOriginalId());
                    continue;
                }
                seenByActualId[actualId] = record;
            }
            toAdd.Add(record);
        }

        context.AddRoadNodeRecords(toAdd);

        return problems;
    }

    private void AddExtractRecordsToContext(List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var changeRoadNodeRecords = context.GetRoadNodeRecords(FeatureType.Change);

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            context.AddRoadNodeRecords([
                new RoadNodeFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractFeature.Attributes.RoadNodeId,
                    RecordType.Identical)
            ]);

            if (extractFeature.Attributes.RoadNodeId < RoadNodeConstants.InitialTemporarySchijnknoopId)
            {
                var hasProcessedRoadNode = changeRoadNodeRecords.Any(x => x.Id == extractFeature.Attributes.RoadNodeId
                                                                          && !x.RecordType.Equals(RecordType.Added));
                if (!hasProcessedRoadNode)
                {
                    context.AddRoadNodeRecords([
                        new RoadNodeFeatureCompareRecord(
                            FeatureType.Change,
                            extractFeature.RecordNumber,
                            extractFeature.Attributes,
                            extractFeature.Attributes.RoadNodeId,
                            RecordType.Removed)
                    ]);
                }
            }
        }
    }
}
