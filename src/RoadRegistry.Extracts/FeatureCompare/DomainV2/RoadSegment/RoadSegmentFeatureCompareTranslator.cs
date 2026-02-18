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

public class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    private readonly IRoadSegmentFeatureCompareStreetNameContextFactory _streetNameContextFactory;
    private readonly IOrganizationCache _organizationCache;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareTranslator(
        RoadSegmentFeatureCompareFeatureReader featureReader,
        IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory,
        IOrganizationCache organizationCache)
        : base(featureReader)
    {
        _streetNameContextFactory = streetNameContextFactory.ThrowIfNull();
        _organizationCache = organizationCache.ThrowIfNull();
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, context);
        problems.ThrowIfError();

        context.RoadSegmentRecords.AddRange(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, feature.Attributes, feature.Attributes.TempId, RecordType.Identical)
        ));

        if (changeFeatures.Any())
        {
            var streetNameContext = await _streetNameContextFactory.Create(changeFeatures, cancellationToken);

            foreach (var feature in context.ChangedRoadSegments.Values)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, feature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), feature.Attributes.TempId);

                problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.LeftSideStreetNameId, true, streetNameContext);
                problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.RightSideStreetNameId, false, streetNameContext);
            }

            (changeFeatures, var maintenanceAuthorityProblems) = await ValidateMaintenanceAuthorityAndMapToInternalId(changeFeatures, cancellationToken);
            problems += maintenanceAuthorityProblems;

            changeFeatures = changeFeatures.Select(EnsureMethodIsNotNull).ToList();

            var batchCount = Debugger.IsAttached ? 1 : 4;

            var processedLeveringRecords = new ConcurrentDictionary<int, (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems)>();
            Parallel.Invoke(changeFeatures
                .SplitIntoBatches(batchCount)
                .Select((changeFeaturesBatch, index) => { return (Action)(() => { processedLeveringRecords.TryAdd(index, ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, streetNameContext, context, cancellationToken)); }); })
                .ToArray());

            foreach (var processedProblems in processedLeveringRecords.OrderBy(x => x.Key).Select(x => x.Value.Item2))
            {
                problems += processedProblems;
            }

            var maxId = integrationFeatures.Select(x => x.Attributes.TempId)
                .Concat(extractFeatures.Select(x => x.Attributes.TempId))
                .Concat(changeFeatures.Select(x => x.Attributes.TempId))
                .Max();

            problems += AddProcessedRecordsToContext(maxId, processedLeveringRecords.OrderBy(x => x.Key).SelectMany(x => x.Value.Item1).ToList(), context, cancellationToken);
        }

        AddRemovedRecordsToContext(extractFeatures, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context, cancellationToken);

        return (changes, problems);
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

    private (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems) ProcessLeveringRecords(
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures,
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures,
        IRoadSegmentFeatureCompareStreetNameContext streetNameContext,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var clusterTolerance = 1.0;

        var problems = ZipArchiveProblems.None;

        var processedRecords = new List<RoadSegmentFeatureCompareRecord>();

        List<Feature<RoadSegmentFeatureCompareAttributes>> FindMatchingExtractFeatures(RoadSegmentFeatureCompareAttributes changeFeatureAttributes)
        {
            if (changeFeatureAttributes.Method == RoadSegmentGeometryDrawMethodV2.Ingeschetst)
            {
                return extractFeatures
                    .Where(x => x.Attributes.TempId == changeFeatureAttributes.TempId)
                    .ToList();
            }

            var bufferedGeometry = changeFeatureAttributes.Geometry!.Buffer(clusterTolerance);
            return extractFeatures
                .Where(x => x.Attributes.Geometry!.Intersects(bufferedGeometry))
                .Where(x => changeFeatureAttributes.Geometry.RoadSegmentOverlapsWith(x.Attributes.Geometry, clusterTolerance))
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

        RoadSegmentFeatureCompareAttributes CorrectStreetNameIds(RoadSegmentFeatureCompareAttributes changeFeatureAttributes)
        {
            return changeFeatureAttributes with
            {
                LeftSideStreetNameId = CorrectStreetNameId(changeFeatureAttributes.LeftSideStreetNameId!.Value),
                RightSideStreetNameId = CorrectStreetNameId(changeFeatureAttributes.RightSideStreetNameId!.Value)
            };
        }

        //TODO-pr FC inwinning changes: FC AddRoadSegment moet bij een merge beide original IDs meegeven (context.ZipArchiveMetadata.Inwinning)
        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changeFeatureAttributes = changeFeature.Attributes;
            var identicalExtractFeature = extractFeatures.FirstOrDefault(e =>
                e.Attributes.TempId == changeFeatureAttributes.TempId
                && e.Attributes.Equals(changeFeatureAttributes));
            if (identicalExtractFeature is not null)
            {
                processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeatureAttributes,
                    changeFeature.Attributes.TempId,
                    RecordType.Identical));
                continue;
            }

            changeFeatureAttributes = CorrectStreetNameIds(changeFeatureAttributes);

            var matchingExtractFeatures = FindMatchingExtractFeatures(changeFeatureAttributes);
            if (matchingExtractFeatures.Any())
            {
                // Test op verschillen in niet kenmerkende attributen
                var nonCriticalAttributesUnchanged = matchingExtractFeatures.FindAll(extractFeature =>
                    changeFeatureAttributes.Status == extractFeature.Attributes.Status &&
                    changeFeatureAttributes.Category == extractFeature.Attributes.Category &&
                    changeFeatureAttributes.LeftSideStreetNameId == extractFeature.Attributes.LeftSideStreetNameId &&
                    changeFeatureAttributes.RightSideStreetNameId == extractFeature.Attributes.RightSideStreetNameId &&
                    changeFeatureAttributes.LeftMaintenanceAuthorityId == extractFeature.Attributes.LeftMaintenanceAuthorityId &&
                    changeFeatureAttributes.RightMaintenanceAuthorityId == extractFeature.Attributes.RightMaintenanceAuthorityId &&
                    changeFeatureAttributes.Method == extractFeature.Attributes.Method &&
                    changeFeatureAttributes.AccessRestriction == extractFeature.Attributes.AccessRestriction &&
                    changeFeatureAttributes.Morphology == extractFeature.Attributes.Morphology
                );
                if (nonCriticalAttributesUnchanged.Any())
                {
                    var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature =>
                        changeFeatureAttributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, context.Tolerances)
                    );
                    if (identicalFeatures.Any())
                    {
                        var extractFeature = identicalFeatures.FirstOrDefault(x => x.Attributes.TempId == changeFeatureAttributes.TempId)
                                             ?? identicalFeatures.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes,
                            extractFeature.Attributes.TempId,
                            RecordType.Identical));
                    }
                    else
                    {
                        //update because geometries differ (slightly)
                        var extractFeature = nonCriticalAttributesUnchanged.FirstOrDefault(x => x.Attributes.TempId == changeFeatureAttributes.TempId)
                                             ?? nonCriticalAttributesUnchanged.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            context.ZipArchiveMetadata.Inwinning
                                ? changeFeatureAttributes
                                : changeFeatureAttributes.OnlyChangedAttributes(extractFeature.Attributes, extractFeature.Attributes.Geometry),
                            extractFeature.Attributes.TempId,
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
                        changeFeatureAttributes.Geometry.IsReasonablyEqualTo(f.Attributes.Geometry, context.Tolerances)
                    );
                    var extractFeature = matchingExtractFeatures.FirstOrDefault(x => x.Attributes.TempId == changeFeatureAttributes.TempId)
                                         ?? matchingExtractFeatures.First();

                    processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        context.ZipArchiveMetadata.Inwinning
                            ? changeFeatureAttributes
                            : changeFeatureAttributes.OnlyChangedAttributes(extractFeature.Attributes, extractFeature.Attributes.Geometry),
                        extractFeature.Attributes.TempId,
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
                changeFeatureAttributes.TempId,
                RecordType.Added));
        }

        return (processedRecords, problems);
    }

    private async Task<(List<Feature<RoadSegmentFeatureCompareAttributes>>, ZipArchiveProblems)> ValidateMaintenanceAuthorityAndMapToInternalId(List<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;
        var result = new List<Feature<RoadSegmentFeatureCompareAttributes>>();

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

        async Task<OrganizationId> ValidateAndGetMaintenanceAuthorityode(Feature<RoadSegmentFeatureCompareAttributes> changeFeature, OrganizationId maintenanceAuthorityCode)
        {
            var maintenanceAuthority = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(maintenanceAuthorityCode, cancellationToken);
            if (maintenanceAuthority is null)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_TEMPID), changeFeature.Attributes.TempId);

                problems += recordContext.RoadSegmentMaintenanceAuthorityNotKnown(maintenanceAuthorityCode);
                return maintenanceAuthorityCode;
            }

            maintenanceAuthorityCode = maintenanceAuthority.Code;
            return maintenanceAuthorityCode;
        }
    }

    private Feature<RoadSegmentFeatureCompareAttributes> EnsureMethodIsNotNull(Feature<RoadSegmentFeatureCompareAttributes> changeFeature)
    {
        if (changeFeature.Attributes.Method is not null)
        {
            return changeFeature;
        }

        //TODO-pr detect Method based on GRB data
        return changeFeature with
        {
            Attributes = changeFeature.Attributes with
            {
                Method = RoadSegmentGeometryDrawMethodV2.Ingemeten
            }
        };
    }

    private ZipArchiveProblems AddProcessedRecordsToContext(RoadSegmentId maxId, ICollection<RoadSegmentFeatureCompareRecord> processedRecords, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        var nextId = maxId.Next();
        foreach (var record in processedRecords
                     .Where(x => x.RecordType.Equals(RecordType.Added)))
        {
            record.Id = nextId;
            nextId = nextId.Next();
        }

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, record.RecordNumber)
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), record.Attributes.TempId);

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

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        //TODO-pr logic merge segments back into 1

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
                        RoadSegmentId = record.Id,
                        OriginalId = record.Id != record.Attributes.TempId
                            ? record.Attributes.TempId
                            : null,
                        Geometry = record.GeometryChanged || context.ZipArchiveMetadata.Inwinning ? geometry : null,
                        GeometryDrawMethod = record.Attributes.Method,
                        AccessRestriction = record.Attributes.AccessRestriction is not null
                            ? new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(record.Attributes.AccessRestriction, geometry)
                            : null,
                        Category = record.Attributes.Category is not null
                            ? new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(record.Attributes.Category, geometry)
                            : null,
                        MaintenanceAuthorityId = BuildOrganizationIdAttributes(record.Attributes.LeftMaintenanceAuthorityId, record.Attributes.RightMaintenanceAuthorityId, geometry),
                        Morphology = record.Attributes.Morphology is not null
                            ? new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(record.Attributes.Morphology, geometry)
                            : null,
                        Status = record.Attributes.Status is not null
                            ? new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(record.Attributes.Status, geometry)
                            : null,
                        SurfaceType = record.Attributes.SurfaceType is not null
                            ? new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(record.Attributes.SurfaceType, geometry)
                            : null,
                        CarAccess = record.Attributes.CarAccess is not null
                            ? new RoadSegmentDynamicAttributeValues<VehicleAccess>(record.Attributes.CarAccess.Value, geometry)
                            : null,
                        BikeAccess = record.Attributes.BikeAccess is not null
                            ? new RoadSegmentDynamicAttributeValues<VehicleAccess>(record.Attributes.BikeAccess.Value, geometry)
                            : null,
                        PedestrianAccess = record.Attributes.PedestrianAccess is not null
                            ? new RoadSegmentDynamicAttributeValues<bool>(record.Attributes.PedestrianAccess.Value, geometry)
                            : null,
                        StreetNameId = BuildStreetNameIdAttributes(record.Attributes.LeftSideStreetNameId, record.Attributes.RightSideStreetNameId, geometry),
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
                            TemporaryId = record.Id,
                            OriginalId = record.Id != record.Attributes.TempId
                                ? record.Attributes.TempId
                                : null,
                            Geometry = geometry,
                            GeometryDrawMethod = record.Attributes.Method!,
                            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(record.Attributes.AccessRestriction!, geometry),
                            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(record.Attributes.Category!, geometry),
                            MaintenanceAuthorityId = BuildOrganizationIdAttributes(record.Attributes.LeftMaintenanceAuthorityId, record.Attributes.RightMaintenanceAuthorityId, geometry)!,
                            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(record.Attributes.Morphology!, geometry),
                            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(record.Attributes.Status!, geometry),
                            StreetNameId = BuildStreetNameIdAttributes(record.Attributes.LeftSideStreetNameId, record.Attributes.RightSideStreetNameId, geometry)!,
                            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(record.Attributes.SurfaceType!, geometry),
                            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(record.Attributes.CarAccess!.Value, geometry),
                            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(record.Attributes.BikeAccess!.Value, geometry),
                            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(record.Attributes.PedestrianAccess!.Value, geometry),
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
                            RoadSegmentId = record.Id
                        }
                    );
                    break;
            }
        }

        return changes;
    }

    private RoadSegmentDynamicAttributeValues<StreetNameLocalId>? BuildStreetNameIdAttributes(StreetNameLocalId? leftSideStreetNameId, StreetNameLocalId? rightSideStreetNameId, RoadSegmentGeometry geometry)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return null;
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(leftSideStreetNameId!.Value, geometry);
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Left, leftSideStreetNameId ?? StreetNameLocalId.NotApplicable)
            .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Right, rightSideStreetNameId ?? StreetNameLocalId.NotApplicable);
    }

    private RoadSegmentDynamicAttributeValues<OrganizationId>? BuildOrganizationIdAttributes(OrganizationId? leftSideOrganizationId, OrganizationId? rightSideOrganizationId, RoadSegmentGeometry geometry)
    {
        if (leftSideOrganizationId is null && rightSideOrganizationId is null)
        {
            return null;
        }

        if (leftSideOrganizationId == rightSideOrganizationId)
        {
            return new RoadSegmentDynamicAttributeValues<OrganizationId>(leftSideOrganizationId!.Value, geometry);
        }

        return new RoadSegmentDynamicAttributeValues<OrganizationId>()
            .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Left, leftSideOrganizationId ?? OrganizationId.Unknown)
            .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Right, rightSideOrganizationId ?? OrganizationId.Unknown);
    }

    private void AddRemovedRecordsToContext(List<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegmentRecords.Any(x => x.Id == extractFeature.Attributes.TempId
                                                                              && !x.RecordType.Equals(RecordType.Added));
            if (!hasProcessedRoadSegment)
            {
                context.RoadSegmentRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractFeature.Attributes.TempId,
                    RecordType.Removed));
            }
        }
    }
}
