namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareWithFlatAttributes>
{
    private readonly IGrbOgcApiFeaturesDownloader _ogcApiFeaturesDownloader;
    private readonly IRoadSegmentFeatureCompareStreetNameContextFactory _streetNameContextFactory;
    private readonly IOrganizationCache _organizationCache;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

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

        context.RoadSegmentRecords.AddRange(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, null, [feature], feature.Attributes.RoadSegmentId!.Value, RecordType.Identical)
        ));

        var maxUsedRoadSegmentId = integrationFeatures.Select(x => x.Attributes.RoadSegmentId!.Value)
            .Concat(extractFeatures.Select(x => x.Attributes.RoadSegmentId!.Value))
            .Concat(changeFeatures.Where(x => x.Attributes.RoadSegmentId is not null).Select(x => x.Attributes.RoadSegmentId!.Value))
            .Max();
        var ogcFeaturesCache = await GetOgcFeaturesCache(context, cancellationToken);
        var dynamicExtractFeaturesTask = Task.Run(() => UnflattenRoadSegments(FeatureType.Extract, extractFeatures, maxUsedRoadSegmentId, ogcFeaturesCache, context, cancellationToken), cancellationToken);

        if (changeFeatures.Any())
        {
            var streetNameContext = await _streetNameContextFactory.Create(changeFeatures, cancellationToken);
            (changeFeatures, var validateProblems) = await ValidateStreetNameAndFixMaintenanceAuthority(changeFeatures, streetNameContext, context, cancellationToken);
            problems += validateProblems;

            //TODO-pr dynamisch maken van attributen
            var dynamicChangeFeaturesTask = Task.Run(() => UnflattenRoadSegments(FeatureType.Change, changeFeatures, maxUsedRoadSegmentId, ogcFeaturesCache, context, cancellationToken), cancellationToken);
            await Task.WhenAll(dynamicChangeFeaturesTask, dynamicExtractFeaturesTask);

            var processedLeveringRecords = ProcessLeveringRecordsInParallel(dynamicChangeFeaturesTask.Result, dynamicExtractFeaturesTask.Result, streetNameContext, context, cancellationToken);

            GenerateNewIdForAddedRecords(processedLeveringRecords, maxUsedRoadSegmentId);

            problems += AddProcessedRecordsToContext(processedLeveringRecords, context, cancellationToken);
        }

        await dynamicExtractFeaturesTask;
        AddRemovedExtractRecordsToContext(dynamicExtractFeaturesTask.Result, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context, cancellationToken);

        return (changes, problems);
    }

    private List<RoadSegmentFeatureCompareRecord> ProcessLeveringRecordsInParallel(
        List<RoadSegmentFeatureWithDynamicAttributes> dynamicChangeFeatures,
        List<RoadSegmentFeatureWithDynamicAttributes> dynamicExtractFeatures,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var batchCount = Debugger.IsAttached ? 1 : 4;

        var processedLeveringRecords = new ConcurrentDictionary<int, List<RoadSegmentFeatureCompareRecord>>();
        Parallel.Invoke(dynamicChangeFeatures
            .SplitIntoBatches(batchCount)
            .Select((changeFeaturesBatch, index) => { return (Action)(() => { processedLeveringRecords.TryAdd(index,
                ProcessLeveringRecords(changeFeaturesBatch, dynamicExtractFeatures, streetNameContext, context, cancellationToken)); }); })
            .ToArray());

        var processedLeveringRecordsList = processedLeveringRecords
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value)
            .ToList();
        return processedLeveringRecordsList;
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
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), feature.Attributes.TempId.ToInt());

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

        //TODO-pr confirm dat die SRID LB08 is
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

    private List<RoadSegmentFeatureCompareRecord> ProcessLeveringRecords(
        ICollection<RoadSegmentFeatureWithDynamicAttributes> changeFeatures,
        ICollection<RoadSegmentFeatureWithDynamicAttributes> extractFeatures,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var processedRecords = new List<RoadSegmentFeatureCompareRecord>();
        const double clusterTolerance = 1.0;

        //TODO-pr FC inwinning changes: FC AddRoadSegment moet bij een merge beide original IDs meegeven (context.ZipArchiveMetadata.Inwinning)

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changeFeatureAttributes = changeFeature.Attributes;
            var identicalExtractFeature = extractFeatures.FirstOrDefault(e =>
                e.Attributes.RoadSegmentId == changeFeatureAttributes.RoadSegmentId
                && e.Attributes.Equals(changeFeatureAttributes));
            if (identicalExtractFeature is not null)
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
                    changeFeatureAttributes.Status == extractFeature.Status &&
                    changeFeatureAttributes.Category == extractFeature.Category &&
                    changeFeatureAttributes.StreetNameId == extractFeature.StreetNameId &&
                    changeFeatureAttributes.MaintenanceAuthorityId == extractFeature.MaintenanceAuthorityId &&
                    changeFeatureAttributes.Method == extractFeature.Method &&
                    changeFeatureAttributes.AccessRestriction == extractFeature.AccessRestriction &&
                    changeFeatureAttributes.Morphology == extractFeature.Morphology
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

        return processedRecords;

        List<RoadSegmentFeatureCompareWithDynamicAttributes> FindMatchingExtractFeatureAttributes(RoadSegmentFeatureCompareWithDynamicAttributes changeFeatureAttributes)
        {
            if (changeFeatureAttributes.Method == RoadSegmentGeometryDrawMethodV2.Ingeschetst)
            {
                return extractFeatures
                    .Where(x => x.Attributes.RoadSegmentId == changeFeatureAttributes.RoadSegmentId)
                    .Select(x => x.Attributes)
                    .ToList();
            }

            var bufferedGeometry = changeFeatureAttributes.Geometry.Buffer(clusterTolerance);
            return extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
                .Where(x => changeFeatureAttributes.Geometry.RoadSegmentOverlapsWith(x.Attributes.Geometry, clusterTolerance))
                .Select(x => x.Attributes)
                .ToList();
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
            var leftMaintenanceAuthorityCode = await ValidateAndGetMaintenanceAuthorityode(changeFeature, changeFeature.Attributes.LeftMaintenanceAuthorityId!.Value);
            var rightMaintenanceAuthorityCode = await ValidateAndGetMaintenanceAuthorityode(changeFeature, changeFeature.Attributes.RightMaintenanceAuthorityId!.Value);

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
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), changeFeature.Attributes.TempId.ToInt());

                problems += recordContext.RoadSegmentMaintenanceAuthorityNotKnown(maintenanceAuthorityCode);
                return maintenanceAuthorityCode;
            }

            maintenanceAuthorityCode = maintenanceAuthority.Code;
            return maintenanceAuthorityCode;
        }
    }

    private RoadSegmentGeometryDrawMethodV2 DetermineMethod(RoadSegmentGeometryDrawMethodV2? method, RoadSegmentStatusV2 status, MultiLineString geometry, OgcFeaturesCache ogcFeaturesCache)
    {
        if (method is not null)
        {
            return method;
        }

        if (status == RoadSegmentStatusV2.Gepland
            || !ogcFeaturesCache.HasOverlapWithFeatures(geometry, 0.75))
        {
            return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
        }

        return RoadSegmentGeometryDrawMethodV2.Ingemeten;
    }

    private ZipArchiveProblems AddProcessedRecordsToContext(
        ICollection<RoadSegmentFeatureCompareRecord> processedRecords,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, record.RecordNumber)
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), record.Attributes.RoadSegmentId);

            var existingRecords = context.RoadSegmentRecords
                .Where(x => x.GetActualId() == record.GetActualId())
                .ToArray();

            if (existingRecords.Length > 1)
            {
                problems += recordContext.IdentifierNotUnique(record.GetActualId(), record.RecordNumber);
                continue;
            }

            var existingRecord = existingRecords.SingleOrDefault();
            if (existingRecord is not null)
            {
                problems += recordContext.RoadSegmentIsAlreadyProcessed(record.GetOriginalId(), existingRecord.GetOriginalId());
                continue;
            }

            context.RoadSegmentRecords.Add(record);
        }

        return problems;
    }

    private static void GenerateNewIdForAddedRecords(ICollection<RoadSegmentFeatureCompareRecord> processedRecords, RoadSegmentId maxId)
    {
        var nextId = maxId.Next();
        foreach (var record in processedRecords
                     .Where(x => x.RecordType.Equals(RecordType.Added)))
        {
            record.RoadSegmentId = nextId;
            nextId = nextId.Next();
        }
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var record in context.RoadSegmentRecords.Where(x => x.FeatureType == FeatureType.Change))
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    break;
                case RecordType.ModifiedIdentifier:
                {
                    var geometry = record.Attributes.Geometry.ToRoadSegmentGeometry();
                    var modifyRoadSegment = new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = record.RoadSegmentId,
                        OriginalId = record.RoadSegmentId != record.Attributes.RoadSegmentId
                            ? record.Attributes.RoadSegmentId
                            : null,
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
                            TemporaryId = record.RoadSegmentId,
                            OriginalId = record.RoadSegmentId != record.Attributes.RoadSegmentId
                                ? record.Attributes.RoadSegmentId
                                : null,
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

    private void AddRemovedExtractRecordsToContext(List<RoadSegmentFeatureWithDynamicAttributes> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegmentRecords.Any(x => x.FeatureType == FeatureType.Change
                                                                              && x.RoadSegmentId == extractFeature.Attributes.RoadSegmentId
                                                                              && x.RecordType != RecordType.Added);
            if (!hasProcessedRoadSegment)
            {
                context.RoadSegmentRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractFeature.FlatFeatures,
                    extractFeature.Attributes.RoadSegmentId,
                    RecordType.Removed));
            }
        }
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> UnflattenRoadSegments(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        RoadSegmentId maxUsedRoadSegmentId,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        // Step 1: Build a graph of road segments and nodes
        var segmentsByNode = BuildSegmentNodeGraph(featureType, records, context);

        // Step 2: Classify nodes according to the rules
        var nodeClassifications = ClassifyNodes(records, segmentsByNode, context);

        // Step 3: Merge segments connected by schijnknopen (nodes with no type assigned)
        var unflattenedRecords = MergeSegmentsAtSchijnknopen(records, maxUsedRoadSegmentId, segmentsByNode, nodeClassifications, context.Tolerances, ogcFeaturesCache, cancellationToken);
        return unflattenedRecords;
    }

    private Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildSegmentNodeGraph(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var segmentsByNode = new Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();

        foreach (var record in records)
        {
            var geometry = record.Attributes.Geometry;
            var startPoint = geometry.Coordinates.First();
            var endPoint = geometry.Coordinates.Last();

            // Find nodes at start and end points
            var startNode = FindNodeAtPoint(featureType, startPoint, context);
            var endNode = FindNodeAtPoint(featureType, endPoint, context);

            if (startNode is not null)
            {
                var key = (startNode.Id, startNode.Attributes.Geometry);
                if (!segmentsByNode.ContainsKey(key))
                {
                    segmentsByNode[key] = [];
                }

                segmentsByNode[key].Add(record);
            }

            if (endNode is not null)
            {
                var key = (endNode.Id, startNode.Attributes.Geometry);
                if (!segmentsByNode.ContainsKey(key))
                {
                    segmentsByNode[key] = [];
                }

                segmentsByNode[key].Add(record);
            }
        }

        return segmentsByNode;
    }

    private RoadNodeFeatureCompareRecord? FindNodeAtPoint(
        FeatureType featuretype,
        Coordinate point,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        return context.RoadNodeRecords
            .FirstOrDefault(x => x.FeatureType == featuretype
                                 && x.RecordType != RecordType.Removed
                                 && x.Attributes.Geometry.IsReasonablyEqualTo(point, context.Tolerances));
    }

    private Dictionary<RoadNodeId, RoadNodeTypeV2> ClassifyNodes(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var nodeClassifications = new Dictionary<RoadNodeId, RoadNodeTypeV2>();

        foreach (var (node, connectedSegments) in segmentsByNode)
        {
            var nodeId = node.Item1;
            var nodeRecord = context.RoadNodeRecords.FirstOrDefault(x => x.Id == nodeId);
            if (nodeRecord is null)
            {
                continue;
            }

            var segmentCount = connectedSegments.Count;

            // Rule 1: eindknoop - connected to exactly 1 segment
            if (segmentCount == 1)
            {
                nodeClassifications[nodeId] = RoadNodeTypeV2.Eindknoop;
                continue;
            }

            // Rule 2: echte knoop - connected to more than 2 segments
            if (segmentCount > 2)
            {
                nodeClassifications[nodeId] = RoadNodeTypeV2.EchteKnoop;
                continue;
            }

            // Rule 3: Connected to exactly 2 segments - check validation node conditions
            if (segmentCount == 2)
            {
                // Rule 3.1: grensknoop = 1
                if (nodeRecord.Attributes.Grensknoop == true)
                {
                    nodeClassifications[nodeId] = RoadNodeTypeV2.Validatieknoop;
                    continue;
                }

                // Rule 3.2: Check if node prevents invalid geometry conditions
                if (PreventsInvalidGeometry(records, nodeRecord.Attributes.Geometry, connectedSegments, context))
                {
                    nodeClassifications[nodeId] = RoadNodeTypeV2.Validatieknoop;
                    continue;
                }

                // If none of the validation conditions apply, it's a schijnknoop
                nodeClassifications[nodeId] = RoadNodeTypeV2.Schijnknoop;
            }
        }

        return nodeClassifications;
    }

    private bool PreventsInvalidGeometry(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Point nodeGeometry,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> connectedSegments,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        if (connectedSegments.Count != 2)
        {
            return false;
        }

        var segment1 = connectedSegments[0];
        var segment2 = connectedSegments[1];

        // Rule 3.2.a: Prevents a segment from crossing itself
        if (segment1.Attributes.Geometry.Intersects(segment2.Attributes.Geometry))
        {
            return true;
        }

        // Rule 3.2.b: Prevents a segment from having same start and end node
        if (GetOtherCoordinate(segment1, nodeGeometry, context).IsReasonablyEqualTo(GetOtherCoordinate(segment2, nodeGeometry, context), context.Tolerances))
        {
            return true;
        }

        // Rule 3.2.c: Check if these two segments would cross multiple times with the same segment without this node
        if (SegmentsCrossMultipleTimes(records, segment1, segment2))
        {
            return true;
        }

        return false;
    }

    private Coordinate GetOtherCoordinate(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment,
        Point nodeGeometry,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var coords = segment.Attributes.Geometry.Coordinates;
        var startPoint = coords.First();
        var endPoint = coords.Last();

        return startPoint.IsReasonablyEqualTo(nodeGeometry.Coordinate, context.Tolerances)
            ? endPoint
            : startPoint;
    }

    private bool SegmentsCrossMultipleTimes(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment1,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment2)
    {
        var geometry1 = segment1.Attributes.Geometry;
        var geometry2 = segment2.Attributes.Geometry;

        var otherIntersectingSegments = records
            .Where(x => x.Attributes.TempId != segment1.Attributes.TempId && x.Attributes.TempId != segment2.Attributes.TempId)
            .Where(x => x.Attributes.Geometry.Intersects(geometry1) && x.Attributes.Geometry.Intersects(geometry2))
            .ToList();

        return otherIntersectingSegments.Any();
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> MergeSegmentsAtSchijnknopen(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        RoadSegmentId maxUsedRoadSegmentId,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        VerificationContextTolerances tolerances,
        OgcFeaturesCache ogcFeaturesCache,
        CancellationToken cancellationToken)
    {
        var result = new List<RoadSegmentFeatureWithDynamicAttributes>();
        var processedSegments = new HashSet<RoadSegmentTempId>();
        var nextRoadSegmentIdProvider = new NextRoadSegmentIdProvider(maxUsedRoadSegmentId);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processedSegments.Contains(record.Attributes.TempId))
            {
                continue;
            }

            // Find all segments connected through schijnknopen
            var segmentChain = BuildSegmentChain(record, segmentsByNode, nodeClassifications, processedSegments, tolerances);

            var dynamicRecord = BuildFeatureWithDynamicAttributes(segmentChain, nextRoadSegmentIdProvider, ogcFeaturesCache);
            result.Add(dynamicRecord);
        }

        return result;
    }

    private sealed class NextRoadSegmentIdProvider
    {
        private RoadSegmentId _nextValue;

        public NextRoadSegmentIdProvider(RoadSegmentId initialValue)
        {
            _nextValue = initialValue;
        }

        public RoadSegmentId Next()
        {
            var result = _nextValue;
            _nextValue = _nextValue.Next();
            return result;
        }
    }

    private List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> BuildSegmentChain(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> startSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        HashSet<RoadSegmentTempId> processedSegments,
        VerificationContextTolerances tolerances)
    {
        var chain = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> { startSegment };
        processedSegments.Add(startSegment.Attributes.TempId);

        // Traverse forward from end point
        TraverseChain(startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward: true, tolerances);

        // Traverse backward from start point
        TraverseChain(startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward: false, tolerances);

        //TODO-pr zorg ervoor dat chain zeker in de juiste volgorde is zodat de merge correct kan
        //TODO-pr merge logica zeker unit testen

        return chain;
    }

    private void TraverseChain(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> currentSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> chain,
        HashSet<RoadSegmentTempId> processedSegments,
        bool isForward,
        VerificationContextTolerances tolerances)
    {
        var nextNodeCoordinate = isForward
            ? currentSegment.Attributes.Geometry.Coordinates.Last()
            : currentSegment.Attributes.Geometry.Coordinates.First();

        var nodeAtPoint = segmentsByNode
            .FirstOrDefault(kvp => kvp.Value.Contains(currentSegment) && kvp.Key.Item2.IsReasonablyEqualTo(nextNodeCoordinate, tolerances))
            .Key;

        if (nodeAtPoint == default)
        {
            return;
        }

        // Check if this node is a schijnknoop
        if (!nodeClassifications.TryGetValue(nodeAtPoint.Item1, out var nodeType) || nodeType != RoadNodeTypeV2.Schijnknoop)
        {
            return;
        }

        // Find the other segment connected to this schijnknoop
        var connectedSegments = segmentsByNode[nodeAtPoint];
        var nextSegment = connectedSegments.FirstOrDefault(s =>
            s.Attributes.TempId != currentSegment.Attributes.TempId && !processedSegments.Contains(s.Attributes.TempId));
        if (nextSegment is null)
        {
            return;
        }

        processedSegments.Add(nextSegment.Attributes.TempId);

        if (isForward)
            chain.Add(nextSegment);
        else
            chain.Insert(0, nextSegment);

        // Continue traversing
        TraverseChain(nextSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward, tolerances);
    }

    private RoadSegmentFeatureWithDynamicAttributes BuildFeatureWithDynamicAttributes(
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> segments,
        NextRoadSegmentIdProvider nextRoadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache)
    {
        var firstSegment = segments.First();

        // Merge geometries
        var mergedGeometry = segments.Count > 1
            ? new MultiLineString([new LineString(segments.SelectMany(s => s.Attributes.Geometry.Coordinates).Distinct().ToArray())])
            : segments.Single().Attributes.Geometry;

        var method = DetermineMethod(firstSegment.Attributes.Method, firstSegment.Attributes.Status!, mergedGeometry, ogcFeaturesCache);
        var dynamicAttributes = RoadSegmentFeatureCompareWithDynamicAttributes.Build(
            firstSegment.Attributes.RoadSegmentId ?? nextRoadSegmentIdProvider.Next(),
            mergedGeometry,
            method,
            segments.Select(x => x.Attributes).ToList());

        return new RoadSegmentFeatureWithDynamicAttributes(
            firstSegment.RecordNumber,
            dynamicAttributes,
            segments);
    }
}

public sealed class OgcFeaturesCache
{
    private readonly IReadOnlyList<OgcFeature> _features;

    public OgcFeaturesCache(IReadOnlyList<OgcFeature> features)
    {
        _features = features;
    }

    public bool HasOverlapWithFeatures(MultiLineString geometry, double minimumOverlapPercentage)
    {
        var source = geometry.Buffer(0.001);
        var overlappingFeatures = _features
            .Select(x => (Feature: x, OverlapPercentage: GetOverlapPercentage(source, x)))
            .Where(x => x.OverlapPercentage > 0)
            .ToList();

        return overlappingFeatures.Sum(x => x.OverlapPercentage) >= minimumOverlapPercentage;
    }

    private static double GetOverlapPercentage(Geometry roadSegmentGeometry, OgcFeature ogcFeature)
    {
        var overlap = roadSegmentGeometry.Intersection(ogcFeature.Geometry);

        var overlapValue = overlap.Area / roadSegmentGeometry.Area;
        return overlapValue;
    }
}
