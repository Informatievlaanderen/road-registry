namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareWithFlatAttributes>
{
    private readonly IGrbOgcApiFeaturesDownloader _ogcApiFeaturesDownloader;
    private readonly IRoadSegmentFeatureCompareStreetNameContextFactory _streetNameContextFactory;
    private readonly IOrganizationCache _organizationCache;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;
    private const double OverlapClusterTolerance = 1.0;

    public RoadSegmentFeatureCompareTranslator(
        RoadSegmentFeatureCompareFeatureReader featureReader,
        IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory,
        IOrganizationCache organizationCache,
        IGrbOgcApiFeaturesDownloader ogcApiFeaturesDownloader)
        : base(featureReader)
    {
        _streetNameContextFactory = streetNameContextFactory;
        _organizationCache = organizationCache;
        _ogcApiFeaturesDownloader = ogcApiFeaturesDownloader;
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, context);
        problems.ThrowIfError();

        context.AddRoadSegmentRecords(integrationFeatures
            .Select(feature =>
                new RoadSegmentFeatureCompareRecord(
                    FeatureType.Integration,
                    feature.RecordNumber,
                    new RoadSegmentFeatureCompareWithDynamicAttributes
                    {
                        RoadSegmentId = feature.Attributes.RoadSegmentId!.Value,
                        Geometry = feature.Attributes.Geometry,
                    },
                    [feature],
                    feature.Attributes.RoadSegmentId!.Value,
                    RecordType.Identical)
            )
            .ToList());

        var maxUsedRoadSegmentId = integrationFeatures.Select(x => x.Attributes.RoadSegmentId!.Value)
            .Concat(extractFeatures.Select(x => x.Attributes.RoadSegmentId!.Value))
            .Concat(changeFeatures.Where(x => x.Attributes.RoadSegmentId is not null).Select(x => x.Attributes.RoadSegmentId!.Value))
            .Max();
        var ogcFeaturesCache = await GetOgcFeaturesCache(context, cancellationToken);
        var dynamicExtractFeaturesTask = Task.Run(() => RoadSegmentUnflattener.Unflatten(FeatureType.Extract, extractFeatures, new ExtractRoadSegmentIdProvider(), ogcFeaturesCache, context, cancellationToken), cancellationToken);

        var streetNameContext = await _streetNameContextFactory.Create(changeFeatures, cancellationToken);
        (changeFeatures, var validateProblems) = await ValidateStreetNameAndFixMaintenanceAuthority(changeFeatures, streetNameContext, context, cancellationToken);
        problems += validateProblems;

        problems += ValidateChangeFeaturesAreWithinTransactionZone(changeFeatures, context);

        var roadSegmentIdProvider = new NextRoadSegmentIdProvider(maxUsedRoadSegmentId);
        var dynamicChangeFeaturesTask = Task.Run(() => RoadSegmentUnflattener.Unflatten(FeatureType.Change, changeFeatures, roadSegmentIdProvider, ogcFeaturesCache, context, cancellationToken), cancellationToken);
        await Task.WhenAll(dynamicChangeFeaturesTask, dynamicExtractFeaturesTask);

        changes = RemoveConsumedSchijnknopen(changes, dynamicChangeFeaturesTask.Result.ConsumedRoadNodeIds);
        var usedExtractRoadSegmentIds = dynamicExtractFeaturesTask.Result.RoadSegments.Select(x => x.Attributes.RoadSegmentId).ToArray();
        var consumedRoadSegmentFlatFeatures = extractFeatures
            .Where(x => !usedExtractRoadSegmentIds.Contains(x.Attributes.RoadSegmentId!.Value))
            .ToArray();
        RemoveConsumedRoadSegments(consumedRoadSegmentFlatFeatures, context);

        var processedLeveringRecords = ProcessLeveringRecordsInParallel(dynamicChangeFeaturesTask.Result.RoadSegments, dynamicExtractFeaturesTask.Result.RoadSegments, streetNameContext, context, cancellationToken);
        problems += processedLeveringRecords.Item2;

        GenerateNewIdForAddedRecords(processedLeveringRecords.Item1, roadSegmentIdProvider);

        await dynamicExtractFeaturesTask;
        FixMultipleReUsesOfRoadSegmentIds(processedLeveringRecords.Item1, dynamicExtractFeaturesTask.Result.RoadSegments, roadSegmentIdProvider, context, cancellationToken);

        context.AddRoadSegmentRecords(processedLeveringRecords.Item1);

        AddExtractRecordsToContext(dynamicExtractFeaturesTask.Result.RoadSegments, context, cancellationToken);
        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context, cancellationToken);

        return (changes, problems);
    }

    private void RemoveConsumedRoadSegments(IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> consumedRoadSegmentFlatFeatures, ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        foreach(var roadSegment in consumedRoadSegmentFlatFeatures.GroupBy(x => x.Attributes.RoadSegmentId!.Value))
        {
            context.AddRoadSegmentRecords([
                new RoadSegmentFeatureCompareRecord(
                    FeatureType.Change,
                    roadSegment.First().RecordNumber,
                    new RoadSegmentFeatureCompareWithDynamicAttributes
                    {
                        RoadSegmentId = roadSegment.Key,
                        Geometry = roadSegment.First().Attributes.Geometry,
                    },
                    roadSegment.ToArray(),
                    roadSegment.Key,
                    RecordType.Removed)
            ]);
        }
    }

    private static TranslatedChanges RemoveConsumedSchijnknopen(TranslatedChanges changes, List<RoadNodeId> consumedRoadNodeIds)
    {
        var roadNodeIdsToRemove = consumedRoadNodeIds
            .Where(x => x < RoadNodeConstants.InitialTemporarySchijnknoopId)
            .ToArray();
        foreach (var roadNodeId in roadNodeIdsToRemove)
        {
            changes = changes.AppendChange(new RemoveRoadNodeChange { RoadNodeId = roadNodeId });
        }

        return changes;
    }

    private static ZipArchiveProblems ValidateChangeFeaturesAreWithinTransactionZone(List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures, ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var problems = ZipArchiveProblems.None;

        foreach (var changeFeature in changeFeatures)
        {
            var shapeRecordContext = ExtractFileName.Wegsegment.AtShapeRecord(FeatureType.Change, changeFeature.RecordNumber)
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), changeFeature.Attributes.TempId.ToInt32());
            problems += context.TransactionZone.Geometry.ValidateGeometryIsAtLeastPartiallyWithinTransactionZone(changeFeature.Attributes.Geometry, shapeRecordContext);
        }

        return problems;
    }

    private (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems) ProcessLeveringRecordsInParallel(
        List<RoadSegmentFeatureWithDynamicAttributes> dynamicChangeFeatures,
        List<RoadSegmentFeatureWithDynamicAttributes> dynamicExtractFeatures,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var batchCount = Debugger.IsAttached ? 1 : 4;

        var spatialIndex = new STRtree<RoadSegmentFeatureWithDynamicAttributes>();
        foreach (var feature in dynamicExtractFeatures)
        {
            spatialIndex.Insert(feature.Attributes.Geometry.EnvelopeInternal, feature);
        }

        spatialIndex.Build();

        var extractFeaturesDictionary = dynamicExtractFeatures.ToDictionary(x => x.Attributes.RoadSegmentId, x => x);

        var processedLeveringRecords = new ConcurrentDictionary<int, (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems)>();
        Parallel.Invoke(dynamicChangeFeatures
            .SplitIntoBatches(batchCount)
            .Select((changeFeaturesBatch, index) =>
            {
                return (Action)(() =>
                {
                    processedLeveringRecords.TryAdd(index,
                        ProcessLeveringRecords(changeFeaturesBatch, extractFeaturesDictionary, spatialIndex, streetNameContext, context, cancellationToken));
                });
            })
            .ToArray());

        var problems = ZipArchiveProblems.None.AddRange(processedLeveringRecords.SelectMany(x => x.Value.Item2));
        var processedLeveringRecordsList = processedLeveringRecords
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value.Item1)
            .ToList();
        return (processedLeveringRecordsList, problems);
    }

    private (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems) ProcessLeveringRecords(
        ICollection<RoadSegmentFeatureWithDynamicAttributes> changeFeatures,
        IDictionary<RoadSegmentId, RoadSegmentFeatureWithDynamicAttributes> extractFeatures,
        STRtree<RoadSegmentFeatureWithDynamicAttributes> spatialIndex,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        var processedRecords = new List<RoadSegmentFeatureCompareRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changeFeatureAttributes = changeFeature.Attributes;
            if (extractFeatures.TryGetValue(changeFeatureAttributes.RoadSegmentId, out var identicalExtractFeature)
                && identicalExtractFeature.Attributes.Equals(changeFeatureAttributes))
            {
                processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeatureAttributes,
                    changeFeature.FlatFeatures,
                    changeFeature.Attributes.RoadSegmentId,
                    RecordType.Identical));
                continue;
            }

            changeFeatureAttributes = CorrectStreetNameIds(changeFeatureAttributes);

            var matchingExtractFeatures = FindMatchingExtractFeatureAttributes(changeFeatureAttributes);
            if (matchingExtractFeatures.Any())
            {
                // Test op verschillen in niet kenmerkende attributen
                var nonCriticalAttributesUnchanged = matchingExtractFeatures.FindAll(extractFeature =>
                    changeFeatureAttributes.Status == extractFeature.Status
                    && changeFeatureAttributes.AccessRestriction == extractFeature.AccessRestriction
                    && changeFeatureAttributes.Category == extractFeature.Category
                    && changeFeatureAttributes.BikeAccessBackward == extractFeature.BikeAccessBackward
                    && changeFeatureAttributes.BikeAccessForward == extractFeature.BikeAccessForward
                    && changeFeatureAttributes.CarAccessBackward == extractFeature.CarAccessBackward
                    && changeFeatureAttributes.CarAccessForward == extractFeature.CarAccessForward
                    && changeFeatureAttributes.MaintenanceAuthorityId == extractFeature.MaintenanceAuthorityId
                    && changeFeatureAttributes.Method == extractFeature.Method
                    && changeFeatureAttributes.Morphology == extractFeature.Morphology
                    && changeFeatureAttributes.PedestrianAccess == extractFeature.PedestrianAccess
                    && changeFeatureAttributes.StreetNameId == extractFeature.StreetNameId
                    && changeFeatureAttributes.SurfaceType == extractFeature.SurfaceType
                );
                if (nonCriticalAttributesUnchanged.Any())
                {
                    var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature =>
                        changeFeatureAttributes.Geometry.IsReasonablyEqualTo(extractFeature.Geometry, context.Tolerances)
                    );
                    if (identicalFeatures.Any())
                    {
                        var extractFeature = identicalFeatures.FirstOrDefault(x => x.RoadSegmentId == changeFeatureAttributes.RoadSegmentId)
                                             ?? identicalFeatures.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes,
                            changeFeature.FlatFeatures,
                            extractFeature.RoadSegmentId,
                            RecordType.Identical));
                    }
                    else
                    {
                        //update because geometries differ (slightly)
                        var extractFeature = nonCriticalAttributesUnchanged.FirstOrDefault(x => x.RoadSegmentId == changeFeatureAttributes.RoadSegmentId)
                                             ?? nonCriticalAttributesUnchanged.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            context.ZipArchiveMetadata.Inwinning
                                ? changeFeatureAttributes
                                : changeFeatureAttributes.OnlyChangedAttributes(extractFeature, extractFeature.Geometry),
                            changeFeature.FlatFeatures,
                            extractFeature.RoadSegmentId,
                            RecordType.Modified)
                        {
                            GeometryChanged = true
                        });
                    }
                }
                else
                {
                    //no features with unchanged non-critical attributes in criticalAttributesUnchanged
                    var identicalGeometries = matchingExtractFeatures.FindAll(f =>
                        changeFeatureAttributes.Geometry.IsReasonablyEqualTo(f.Geometry, context.Tolerances)
                    );
                    var extractFeature = matchingExtractFeatures.FirstOrDefault(x => x.RoadSegmentId == changeFeatureAttributes.RoadSegmentId)
                                         ?? matchingExtractFeatures.First();

                    processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        context.ZipArchiveMetadata.Inwinning
                            ? changeFeatureAttributes
                            : changeFeatureAttributes.OnlyChangedAttributes(extractFeature, extractFeature.Geometry),
                        changeFeature.FlatFeatures,
                        extractFeature.RoadSegmentId,
                        RecordType.Modified)
                    {
                        GeometryChanged = !identicalGeometries.Any()
                    });
                }

                continue;
            }

            processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                FeatureType.Change,
                changeFeature.RecordNumber,
                changeFeatureAttributes,
                changeFeature.FlatFeatures,
                changeFeatureAttributes.RoadSegmentId,
                RecordType.Added));
        }

        return (processedRecords, problems);

        List<RoadSegmentFeatureCompareWithDynamicAttributes> FindMatchingExtractFeatureAttributes(RoadSegmentFeatureCompareWithDynamicAttributes changeFeatureAttributes)
        {
            if (changeFeatureAttributes.Status == RoadSegmentStatusV2.Gerealiseerd)
            {
                var bufferedGeometry = changeFeatureAttributes.Geometry.Buffer(OverlapClusterTolerance);
                return spatialIndex
                    .Query(bufferedGeometry.EnvelopeInternal)
                    .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
                    .Where(x => changeFeatureAttributes.Geometry.RoadSegmentOverlapsWith(x.Attributes.Geometry, OverlapClusterTolerance))
                    .Select(x => x.Attributes)
                    .ToList();
            }

            if (extractFeatures.TryGetValue(changeFeatureAttributes.RoadSegmentId, out var extractFeature))
            {
                return [extractFeature.Attributes];
            }

            return [];
        }

        StreetNameLocalId CorrectStreetNameId(StreetNameLocalId id)
        {
            if (id > 0)
            {
                if (streetNameContext.IsRemoved(id))
                {
                    return StreetNameLocalId.NotApplicable;
                }

                if (streetNameContext.TryGetRenamedId(id, out var renamedToId))
                {
                    return renamedToId;
                }
            }

            return id;
        }

        RoadSegmentFeatureCompareWithDynamicAttributes CorrectStreetNameIds(RoadSegmentFeatureCompareWithDynamicAttributes changeFeatureAttributes)
        {
            if (changeFeatureAttributes.StreetNameId is not null)
            {
                return changeFeatureAttributes with
                {
                    StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(changeFeatureAttributes.StreetNameId.Values
                        .Select(x => (x.Coverage, x.Side, CorrectStreetNameId(x.Value))))
                };
            }

            return changeFeatureAttributes;
        }
    }

    private async Task<(List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>, ZipArchiveProblems)> ValidateMaintenanceAuthorityAndMapToInternalId(List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;
        var result = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();

        foreach (var changeFeature in changeFeatures)
        {
            var leftMaintenanceAuthorityCode = await ValidateAndGetMaintenanceAuthorityode(changeFeature, changeFeature.Attributes.LeftMaintenanceAuthorityId);
            var rightMaintenanceAuthorityCode = await ValidateAndGetMaintenanceAuthorityode(changeFeature, changeFeature.Attributes.RightMaintenanceAuthorityId);

            result.Add(changeFeature with
            {
                Attributes = changeFeature.Attributes with
                {
                    LeftMaintenanceAuthorityId = leftMaintenanceAuthorityCode,
                    RightMaintenanceAuthorityId = rightMaintenanceAuthorityCode
                }
            });
        }

        return (result, problems);

        async Task<OrganizationId> ValidateAndGetMaintenanceAuthorityode(Feature<RoadSegmentFeatureCompareWithFlatAttributes> changeFeature, OrganizationId maintenanceAuthorityCode)
        {
            var maintenanceAuthority = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(maintenanceAuthorityCode, cancellationToken);
            if (maintenanceAuthority is null)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), changeFeature.Attributes.TempId.ToInt32());

                problems += recordContext.RoadSegmentMaintenanceAuthorityNotKnown(maintenanceAuthorityCode);
                return maintenanceAuthorityCode;
            }

            maintenanceAuthorityCode = maintenanceAuthority.Code;
            return maintenanceAuthorityCode;
        }
    }

    private static void GenerateNewIdForAddedRecords(ICollection<RoadSegmentFeatureCompareRecord> processedRecords, IRoadSegmentIdProvider roadSegmentIdProvider)
    {
        foreach (var record in processedRecords
                     .Where(x => x.RecordType.Equals(RecordType.Added)))
        {
            record.RoadSegmentId = roadSegmentIdProvider.NewId();
        }
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var record in context.GetRoadSegmentRecords(FeatureType.Change))
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.ModifiedIdentifier:
                {
                    var geometry = record.Attributes.Geometry.ToRoadSegmentGeometry();
                    var modifyRoadSegment = new ModifyRoadSegmentChange
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(record.RoadSegmentId, record.FlatFeatures.Select(x => x.Attributes.TempId).ToArray()),
                        Geometry = record.GeometryChanged || context.ZipArchiveMetadata.Inwinning ? geometry : null,
                        GeometryDrawMethod = record.Attributes.Method,
                        Status = record.Attributes.Status,
                        AccessRestriction = record.Attributes.AccessRestriction,
                        Category = record.Attributes.Category,
                        MaintenanceAuthorityId = record.Attributes.MaintenanceAuthorityId,
                        Morphology = record.Attributes.Morphology,
                        StreetNameId = record.Attributes.StreetNameId,
                        SurfaceType = record.Attributes.SurfaceType,
                        CarAccessForward = record.Attributes.CarAccessForward,
                        CarAccessBackward = record.Attributes.CarAccessBackward,
                        BikeAccessForward = record.Attributes.BikeAccessForward,
                        BikeAccessBackward = record.Attributes.BikeAccessBackward,
                        PedestrianAccess = record.Attributes.PedestrianAccess
                    };

                    changes = changes.AppendChange(modifyRoadSegment);
                }
                    break;
                case RecordType.AddedIdentifier:
                {
                    var geometry = record.Attributes.Geometry.ToRoadSegmentGeometry();
                    changes = changes.AppendChange(
                        new AddRoadSegmentChange
                        {
                            RoadSegmentIdReference = new RoadSegmentIdReference(record.RoadSegmentId, record.FlatFeatures.Select(x => x.Attributes.TempId).ToArray()),
                            Geometry = geometry,
                            GeometryDrawMethod = record.Attributes.Method!,
                            Status = record.Attributes.Status!,
                            AccessRestriction = record.Attributes.AccessRestriction!,
                            Category = record.Attributes.Category!,
                            MaintenanceAuthorityId = record.Attributes.MaintenanceAuthorityId!,
                            Morphology = record.Attributes.Morphology!,
                            StreetNameId = record.Attributes.StreetNameId!,
                            SurfaceType = record.Attributes.SurfaceType!,
                            CarAccessForward = record.Attributes.CarAccessForward!,
                            CarAccessBackward = record.Attributes.CarAccessBackward!,
                            BikeAccessForward = record.Attributes.BikeAccessForward!,
                            BikeAccessBackward = record.Attributes.BikeAccessBackward!,
                            PedestrianAccess = record.Attributes.PedestrianAccess!,
                            EuropeanRoadNumbers = [],
                            NationalRoadNumbers = []
                        }
                    );
                }
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegmentChange
                        {
                            RoadSegmentId = record.RoadSegmentId
                        }
                    );
                    break;
            }
        }

        return changes;
    }

    private void FixMultipleReUsesOfRoadSegmentIds(
        IReadOnlyCollection<RoadSegmentFeatureCompareRecord> changeRecords,
        List<RoadSegmentFeatureWithDynamicAttributes> extractFeatures,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var changedRoadSegmentsWhoReUseTheSameExtractRoadSegmentId = changeRecords
            .Where(x => x.RecordType != RecordType.Removed)
            .GroupBy(x => x.GetActualId())
            .Where(x => x.Count() > 1)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (changedRoadSegmentsWhoReUseTheSameExtractRoadSegmentId.TryGetValue(extractFeature.Attributes.RoadSegmentId, out var matchedChangedFeatures))
            {
                var matchingFeaturesSortedByOverlapPercentage = matchedChangedFeatures
                    .OrderByDescending(x => x.Attributes.Geometry.CalculateOverlapPercentage(extractFeature.Attributes.Geometry, OverlapClusterTolerance))
                    .ToList();

                var featureWhoGetsTheExtractId = matchingFeaturesSortedByOverlapPercentage.First();
                featureWhoGetsTheExtractId.RoadSegmentId = extractFeature.Attributes.RoadSegmentId;
                if (featureWhoGetsTheExtractId.RecordType != RecordType.Identical)
                {
                    featureWhoGetsTheExtractId.RecordType = RecordType.Modified;
                    featureWhoGetsTheExtractId.GeometryChanged = !featureWhoGetsTheExtractId.Attributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, context.Tolerances);
                }

                foreach (var matchedFeature in matchingFeaturesSortedByOverlapPercentage.Skip(1))
                {
                    matchedFeature.RoadSegmentId = roadSegmentIdProvider.NewId();
                    matchedFeature.RecordType = RecordType.Added;
                    matchedFeature.GeometryChanged = false;
                }
            }
        }
    }

    private void AddExtractRecordsToContext(
        List<RoadSegmentFeatureWithDynamicAttributes> extractFeatures,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var changeRoadSegmentRecords = context.GetRoadSegmentRecords(FeatureType.Change);

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            context.AddRoadSegmentRecords([
                new RoadSegmentFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractFeature.FlatFeatures,
                    extractFeature.Attributes.RoadSegmentId,
                    RecordType.Identical)
            ]);

            var hasProcessedRoadSegment = changeRoadSegmentRecords.Any(x =>
                x.GetActualId() == extractFeature.Attributes.RoadSegmentId && x.RecordType != RecordType.Added);
            if (!hasProcessedRoadSegment)
            {
                context.AddRoadSegmentRecords([
                    new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        extractFeature.RecordNumber,
                        extractFeature.Attributes,
                        extractFeature.FlatFeatures,
                        extractFeature.Attributes.RoadSegmentId,
                        RecordType.Removed)
                ]);
            }
        }
    }

    private async Task<(List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures, ZipArchiveProblems problems)> ValidateStreetNameAndFixMaintenanceAuthority(
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        if (!changeFeatures.Any())
        {
            return (changeFeatures, ZipArchiveProblems.None);
        }

        var problems = ZipArchiveProblems.None;

        foreach (var feature in context.ChangedRoadSegments.Values)
        {
            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, feature.RecordNumber)
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), feature.Attributes.TempId.ToInt32());

            problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.LeftSideStreetNameId, true, streetNameContext);
            problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.RightSideStreetNameId, false, streetNameContext);
        }

        (changeFeatures, var maintenanceAuthorityProblems) = await ValidateMaintenanceAuthorityAndMapToInternalId(changeFeatures, cancellationToken);
        problems += maintenanceAuthorityProblems;
        return (changeFeatures, problems);
    }

    private async Task<OgcFeaturesCache> GetOgcFeaturesCache(ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        if (!context.ZipArchiveMetadata.Inwinning)
        {
            return new OgcFeaturesCache([]);
        }

        var ogcFeatures = await _ogcApiFeaturesDownloader.DownloadFeaturesAsync(
            ["KNW", "WBN"],
            context.TransactionZone.Geometry.Value.Boundary.EnvelopeInternal,
            context.TransactionZone.Geometry.Value.SRID,
            cancellationToken);

        return new OgcFeaturesCache(ogcFeatures);
    }

    private static ZipArchiveProblems GetProblemsForStreetNameId(IDbaseFileRecordProblemBuilder recordContext, StreetNameLocalId? id, bool leftSide, IRoadSegmentFeatureCompareStreetNameContext streetNameContext)
    {
        var problems = ZipArchiveProblems.None;

        if (id > 0)
        {
            if (!streetNameContext.IsValid(id.Value))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdOutOfRange(id.Value)
                    : recordContext.RightStreetNameIdOutOfRange(id.Value));
            }

            if (streetNameContext.IsRemoved(id.Value))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRemoved(id.Value)
                    : recordContext.RightStreetNameIdIsRemoved(id.Value));
            }

            if (streetNameContext.TryGetRenamedId(id.Value, out var renamedToId))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRenamed(id.Value, renamedToId)
                    : recordContext.RightStreetNameIdIsRenamed(id.Value, renamedToId));
            }
        }

        return problems;
    }

    private sealed class ExtractRoadSegmentIdProvider : IRoadSegmentIdProvider
    {
        public RoadSegmentId NewId()
        {
            throw new InvalidOperationException("It should not be needed to generate a new ID for extract road segments.");
        }
    }
}
