namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadNode;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNode.Changes;
using TranslatedChanges = Inwinning.TranslatedChanges;

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
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, context);
        problems.ThrowIfError();

        if (changeFeatures.Any())
        {
            var newRoadNodeIds = GenerateTemporaryIdsForRoadNodesWithoutId(changeFeatures, extractFeatures, integrationFeatures);

            var processedLeveringRecords = ProcessLeveringRecordsInParallel(
                changeFeatures, extractFeatures, newRoadNodeIds, context, cancellationToken);

            problems += AddProcessedRecordsToContext(processedLeveringRecords, context, cancellationToken);
        }

        AddExtractRecordsToContext(extractFeatures, context, cancellationToken);
        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context, cancellationToken);

        return Task.FromResult((changes, problems));
    }

    private static IReadOnlyDictionary<RecordNumber, RoadNodeId> GenerateTemporaryIdsForRoadNodesWithoutId(
        List<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures,
        List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures,
        List<Feature<RoadNodeFeatureCompareAttributes>> integrationFeatures)
    {
        // the ids of temporary schijnknopen are ignored, so a new road node never ends up in their range
        var nextRoadNodeId = changeFeatures
            .Concat(extractFeatures)
            .Concat(integrationFeatures)
            .Where(x => x.Attributes.RoadNodeId is not null && x.Attributes.RoadNodeId < RoadNodeConstants.InitialTemporarySchijnknoopId)
            .Select(x => x.Attributes.RoadNodeId!.Value)
            .DefaultIfEmpty(RoadNodeId.Zero)
            .Max()
            .Next();

        var newRoadNodeIds = new Dictionary<RecordNumber, RoadNodeId>();

        // change features are in record order, so the generated ids are deterministic
        foreach (var changeFeature in changeFeatures.Where(x => x.Attributes.RoadNodeId is null))
        {
            newRoadNodeIds.Add(changeFeature.RecordNumber, nextRoadNodeId);
            nextRoadNodeId = nextRoadNodeId.Next();
        }

        return newRoadNodeIds;
    }

    private List<RoadNodeFeatureCompareRecord> ProcessLeveringRecordsInParallel(
        List<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures,
        List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures,
        IReadOnlyDictionary<RecordNumber, RoadNodeId> newRoadNodeIds,
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
            .ToDictionary(x => x.Attributes.RoadNodeId!.Value, x => x);

        var processedLeveringRecords = new ConcurrentDictionary<int, List<RoadNodeFeatureCompareRecord>>();
        Parallel.Invoke(changeFeatures
            .SplitIntoBatches(batchCount)
            .Select((changeFeaturesBatch, index) =>
            {
                return (Action)(() =>
                {
                    processedLeveringRecords.TryAdd(index,
                        ProcessLeveringRecords(changeFeaturesBatch, extractFeaturesDictionary, spatialIndex, newRoadNodeIds, context, cancellationToken));
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
        IReadOnlyDictionary<RecordNumber, RoadNodeId> newRoadNodeIds,
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
                    var extractRoadNodeId = extractFeature.Attributes.RoadNodeId!.Value;
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        extractRoadNodeId,
                        RecordType.Identical));
                    var isTemporarySchijnknoop = extractRoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractRoadNodeId, 0);
                    }
                }
                else
                {
                    var extractFeature = intersectingGeometries.First();
                    var extractRoadNodeId = extractFeature.Attributes.RoadNodeId!.Value;
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        extractRoadNodeId,
                        RecordType.Modified)
                    {
                        GeometryChanged = true
                    });
                    var isTemporarySchijnknoop = extractRoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractRoadNodeId, 0);
                    }
                }
            }
            else
            {
                if (changeFeature.Attributes.RoadNodeId is not null
                    && extractFeatures.TryGetValue(changeFeature.Attributes.RoadNodeId.Value, out var extractFeature))
                {
                    var extractRoadNodeId = extractFeature.Attributes.RoadNodeId!.Value;
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        extractFeature.RecordNumber,
                        extractFeature.Attributes,
                        extractRoadNodeId,
                        RecordType.Removed));
                    var isTemporarySchijnknoop = extractRoadNodeId >= RoadNodeConstants.InitialTemporarySchijnknoopId;
                    if (isTemporarySchijnknoop)
                    {
                        context.TemporarySchijnknoopIds.TryAdd(extractRoadNodeId, 0);
                    }
                }

                processedRecords.Add(new RoadNodeFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeature.Attributes,
                    changeFeature.Attributes.RoadNodeId ?? newRoadNodeIds[changeFeature.RecordNumber],
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
                    // An inwinning takes over the nodes it leaves untouched, so they migrate along with their road segments
                    changes = changes.AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = record.Id,
                            Geometry = record.Attributes.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = record.Attributes.Grensknoop
                        }
                    );
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
                            Geometry = record.Attributes.Geometry.ToRoadNodeGeometry(),
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
                    problems += recordContext.RoadNodeIsAlreadyProcessed(record.GetOriginalId(), existing.GetOriginalId(), existing.RecordNumber);
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

            var extractRoadNodeId = extractFeature.Attributes.RoadNodeId!.Value;
            context.AddRoadNodeRecords([
                new RoadNodeFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractRoadNodeId,
                    RecordType.Identical)
            ]);

            if (extractRoadNodeId < RoadNodeConstants.InitialTemporarySchijnknoopId)
            {
                var hasProcessedRoadNode = changeRoadNodeRecords.Any(x => x.Id == extractRoadNodeId
                                                                          && !x.RecordType.Equals(RecordType.Added));
                if (!hasProcessedRoadNode)
                {
                    context.AddRoadNodeRecords([
                        new RoadNodeFeatureCompareRecord(
                            FeatureType.Change,
                            extractFeature.RecordNumber,
                            extractFeature.Attributes,
                            extractRoadNodeId,
                            RecordType.Removed)
                    ]);
                }
            }
        }
    }
}
