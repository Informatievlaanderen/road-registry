namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        context.AddRoadSegments(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, null, [feature], feature.Attributes.RoadSegmentId!.Value, RecordType.Identical)
        ));

        var maxUsedRoadSegmentId = integrationFeatures.Select(x => x.Attributes.RoadSegmentId!.Value)
            .Concat(extractFeatures.Select(x => x.Attributes.RoadSegmentId!.Value))
            .Concat(changeFeatures.Where(x => x.Attributes.RoadSegmentId is not null).Select(x => x.Attributes.RoadSegmentId!.Value))
            .Max();
        var ogcFeaturesCache = await GetOgcFeaturesCache(context, cancellationToken);
        var dynamicExtractFeaturesTask = Task.Run(() => RoadSegmentUnflattener.Unflatten(FeatureType.Extract, extractFeatures, maxUsedRoadSegmentId, ogcFeaturesCache, context, cancellationToken), cancellationToken);

        if (changeFeatures.Any())
        {
            var streetNameContext = await _streetNameContextFactory.Create(changeFeatures, cancellationToken);
            (changeFeatures, var validateProblems) = await ValidateStreetNameAndFixMaintenanceAuthority(changeFeatures, streetNameContext, context, cancellationToken);
            problems += validateProblems;

            var dynamicChangeFeaturesTask = Task.Run(() => RoadSegmentUnflattener.Unflatten(FeatureType.Change, changeFeatures, maxUsedRoadSegmentId, ogcFeaturesCache, context, cancellationToken), cancellationToken);
            await Task.WhenAll(dynamicChangeFeaturesTask, dynamicExtractFeaturesTask);

            var processedLeveringRecords = ProcessLeveringRecordsInParallel(dynamicChangeFeaturesTask.Result, dynamicExtractFeaturesTask.Result, streetNameContext, context, cancellationToken);

            GenerateNewIdForAddedRecords(processedLeveringRecords, maxUsedRoadSegmentId);

            problems += AddProcessedRecordsToContext(processedLeveringRecords, context, cancellationToken);
        }

        await dynamicExtractFeaturesTask;
        AddExtractRecordsToContext(dynamicExtractFeaturesTask.Result, context, cancellationToken);

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
            .Select((changeFeaturesBatch, index) =>
            {
                return (Action)(() =>
                {
                    processedLeveringRecords.TryAdd(index,
                        ProcessLeveringRecords(changeFeaturesBatch, dynamicExtractFeatures, streetNameContext, context, cancellationToken));
                });
            })
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
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), changeFeature.Attributes.TempId.ToInt());

                problems += recordContext.RoadSegmentMaintenanceAuthorityNotKnown(maintenanceAuthorityCode);
                return maintenanceAuthorityCode;
            }

            maintenanceAuthorityCode = maintenanceAuthority.Code;
            return maintenanceAuthorityCode;
        }
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

            context.AddRoadSegments([record]);
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

    private void AddExtractRecordsToContext(List<RoadSegmentFeatureWithDynamicAttributes> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegmentRecords.Any(x => x.FeatureType == FeatureType.Change
                                                                              && x.RoadSegmentId == extractFeature.Attributes.RoadSegmentId
                                                                              && x.RecordType != RecordType.Added);

            context.AddRoadSegments([new RoadSegmentFeatureCompareRecord(
                FeatureType.Extract,
                extractFeature.RecordNumber,
                extractFeature.Attributes,
                extractFeature.FlatFeatures,
                extractFeature.Attributes.RoadSegmentId,
                hasProcessedRoadSegment ? RecordType.Identical : RecordType.Removed)]);
        }
    }
}
