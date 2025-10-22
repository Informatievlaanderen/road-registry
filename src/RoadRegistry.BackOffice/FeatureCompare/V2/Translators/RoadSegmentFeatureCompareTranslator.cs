namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using NetTopologySuite.Geometries;
using Readers;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Uploads;
using RoadSegment.ValueObjects;
using AddRoadSegment = Uploads.AddRoadSegment;
using ModifyRoadSegment = Uploads.ModifyRoadSegment;
using RemoveOutlinedRoadSegment = Uploads.RemoveOutlinedRoadSegment;
using RemoveRoadSegment = Uploads.RemoveRoadSegment;

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
            if (changeFeatureAttributes.Method == RoadSegmentGeometryDrawMethod.Outlined)
            {
                return extractFeatures
                    .Where(x => x.Attributes.Id == changeFeatureAttributes.Id)
                    .ToList();
            }

            var bufferedGeometry = changeFeatureAttributes.Geometry!.Buffer(clusterTolerance);
            return extractFeatures
                .Where(x => x.Attributes.Geometry!.Intersects(bufferedGeometry))
                .Where(x => changeFeatureAttributes.Geometry.RoadSegmentOverlapsWith(x.Attributes.Geometry, clusterTolerance))
                .ToList();
        }

        RoadNodeFeatureCompareRecord FindRoadNodeByOriginalId(RoadNodeId originalId)
        {
            var matchingFeatures = context.RoadNodeRecords
                .NotRemoved()
                .Where(x => x.GetOriginalId() == originalId)
                .ToList();

            if (matchingFeatures.Count > 1)
            {
                var matchingFeaturesInfo = string.Join("\n", matchingFeatures.Select(feature => $"RoadNode #{feature.RecordNumber}, ID: {feature.Id}, FeatureType: {feature.FeatureType}, RecordType: {feature.RecordType}"));
                throw new InvalidOperationException($"Found {matchingFeatures.Count} processed road nodes with original ID {originalId} while only 1 is expected.\n{matchingFeaturesInfo}");
            }

            return matchingFeatures.SingleOrDefault();
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

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changeFeatureAttributes = changeFeature.Attributes;
            var identicalExtractFeature = extractFeatures.FirstOrDefault(e =>
                e.Attributes.Id == changeFeatureAttributes.Id
                && e.Attributes.Equals(changeFeatureAttributes));
            if (identicalExtractFeature is not null)
            {
                processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeatureAttributes,
                    changeFeature.Attributes.Id,
                    RecordType.Identical));
                continue;
            }

            var alwaysIncludeNodeIdsInChangedAttributes = false;
            if (changeFeatureAttributes.StartNodeId > 0 && changeFeatureAttributes.EndNodeId > 0)
            {
                var startNodeFeature = FindRoadNodeByOriginalId(changeFeatureAttributes.StartNodeId!.Value);
                var endNodeFeature = FindRoadNodeByOriginalId(changeFeatureAttributes.EndNodeId!.Value);

                if (startNodeFeature is null || endNodeFeature is null)
                {
                    var recordContext = FileName
                        .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                        .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), changeFeature.Attributes.Id);

                    if (startNodeFeature is null)
                    {
                        problems += recordContext.BeginRoadNodeIdOutOfRange(changeFeatureAttributes.StartNodeId!.Value);
                    }

                    if (endNodeFeature is null)
                    {
                        problems += recordContext.EndRoadNodeIdOutOfRange(changeFeatureAttributes.EndNodeId!.Value);
                    }
                    continue;
                }

                if (startNodeFeature.RecordType == RecordType.Added || endNodeFeature.RecordType == RecordType.Added)
                {
                    alwaysIncludeNodeIdsInChangedAttributes = true;
                }

                changeFeatureAttributes = changeFeatureAttributes with
                {
                    StartNodeId = startNodeFeature.GetActualId(),
                    EndNodeId = endNodeFeature.GetActualId()
                };
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
                    changeFeatureAttributes.MaintenanceAuthority == extractFeature.Attributes.MaintenanceAuthority &&
                    changeFeatureAttributes.Method == extractFeature.Attributes.Method &&
                    changeFeatureAttributes.StartNodeId == extractFeature.Attributes.StartNodeId &&
                    changeFeatureAttributes.EndNodeId == extractFeature.Attributes.EndNodeId &&
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
                        var extractFeature = identicalFeatures.FirstOrDefault(x => x.Attributes.Id == changeFeatureAttributes.Id)
                                             ?? identicalFeatures.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes,
                            extractFeature.Attributes.Id,
                            RecordType.Identical));
                    }
                    else
                    {
                        //update because geometries differ (slightly)
                        var extractFeature = nonCriticalAttributesUnchanged.FirstOrDefault(x => x.Attributes.Id == changeFeatureAttributes.Id)
                                             ?? nonCriticalAttributesUnchanged.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes.OnlyChangedAttributes(extractFeature.Attributes, extractFeature.Attributes.Geometry, alwaysIncludeNodeIdsInChangedAttributes),
                            extractFeature.Attributes.Id,
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
                    var extractFeature = matchingExtractFeatures.FirstOrDefault(x => x.Attributes.Id == changeFeatureAttributes.Id)
                                         ?? matchingExtractFeatures.First();

                    var convertedFromOutlined = extractFeature.Attributes.Method == RoadSegmentGeometryDrawMethod.Outlined
                                                && changeFeatureAttributes.Method != extractFeature.Attributes.Method;

                    processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        convertedFromOutlined ? changeFeatureAttributes : changeFeatureAttributes.OnlyChangedAttributes(extractFeature.Attributes, extractFeature.Attributes.Geometry, alwaysIncludeNodeIdsInChangedAttributes),
                        extractFeature.Attributes.Id,
                        RecordType.Modified)
                    {
                        GeometryChanged = !identicalGeometries.Any(),
                        ConvertedFromOutlined = convertedFromOutlined
                    });
                }

                continue;
            }

            processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                FeatureType.Change,
                changeFeature.RecordNumber,
                changeFeatureAttributes,
                changeFeatureAttributes.Id,
                RecordType.Added));
        }

        return (processedRecords, problems);
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, context);

        problems.ThrowIfError();

        context.RoadSegmentRecords.AddRange(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, feature.Attributes, feature.Attributes.Id, RecordType.Identical)
        ));

        if (changeFeatures.Any())
        {
            (changeFeatures, var maintenanceAuthorityProblems) = await ValidateMaintenanceAuthorityAndMapToInternalId(changeFeatures, cancellationToken);
            problems += maintenanceAuthorityProblems;

            var streetNameContext = await _streetNameContextFactory.Create(changeFeatures, cancellationToken);

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

            var maxId = integrationFeatures.Select(x => x.Attributes.Id)
                .Concat(extractFeatures.Select(x => x.Attributes.Id))
                .Concat(changeFeatures.Select(x => x.Attributes.Id))
                .Max();

            problems += AddProcessedRecordsToContext(maxId, processedLeveringRecords.OrderBy(x => x.Key).SelectMany(x => x.Value.Item1).ToList(), context, cancellationToken);
        }

        AddRemovedRecordsToContext(extractFeatures, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context.RoadSegmentRecords, cancellationToken);

        return (changes, problems);
    }

    private async Task<(List<Feature<RoadSegmentFeatureCompareAttributes>>, ZipArchiveProblems)> ValidateMaintenanceAuthorityAndMapToInternalId(List<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;
        var result = new List<Feature<RoadSegmentFeatureCompareAttributes>>();

        foreach (var changeFeature in changeFeatures)
        {
            var maintenanceAuthorityCode = changeFeature.Attributes.MaintenanceAuthority!.Value;

            var maintenanceAuthority = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(maintenanceAuthorityCode, cancellationToken);
            if (maintenanceAuthority is null)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), changeFeature.Attributes.Id);

                problems += recordContext.RoadSegmentMaintenanceAuthorityNotKnown(maintenanceAuthorityCode);
                continue;
            }

            maintenanceAuthorityCode = maintenanceAuthority.Code;

            result.Add(changeFeature with
            {
                Attributes = changeFeature.Attributes with
                {
                    MaintenanceAuthority = maintenanceAuthorityCode
                }
            });
        }

        return (result, problems);
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
                .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), record.Attributes.Id);

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

            MigrateRoadNodeIds(record, context);

            context.RoadSegmentRecords.Add(record);
        }

        return problems;
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<RoadSegmentFeatureCompareRecord> records, CancellationToken cancellationToken)
    {
        foreach (var record in records.Where(x => x.FeatureType != FeatureType.Integration))
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    changes = changes.AppendProvisionalChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Method
                        )
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    var modifyRoadSegment = new ModifyRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Method,
                            record.Attributes.StartNodeId,
                            record.Attributes.EndNodeId,
                            record.Attributes.MaintenanceAuthority,
                            record.Attributes.Morphology,
                            record.Attributes.Status,
                            record.Attributes.Category,
                            record.Attributes.AccessRestriction,
                            record.Attributes.LeftSideStreetNameId,
                            record.Attributes.RightSideStreetNameId,
                            geometry: record.GeometryChanged || record.ConvertedFromOutlined ? record.Attributes.Geometry : null
                        )
                        .WithConvertedFromOutlined(record.ConvertedFromOutlined);
                    if (record.Id != record.Attributes.Id)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithOriginalId(record.Attributes.Id);
                    }

                    changes = changes.AppendChange(modifyRoadSegment);

                    if (record.ConvertedFromOutlined)
                    {
                        changes = changes.AppendChange(
                            new RemoveOutlinedRoadSegment(
                                record.RecordNumber,
                                record.Id
                            )
                        );
                    }
                    break;
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Id,
                            record.Attributes.StartNodeId!.Value,
                            record.Attributes.EndNodeId!.Value,
                            record.Attributes.MaintenanceAuthority!.Value,
                            record.Attributes.Method,
                            record.Attributes.Morphology,
                            record.Attributes.Status,
                            record.Attributes.Category,
                            record.Attributes.AccessRestriction,
                            record.Attributes.LeftSideStreetNameId,
                            record.Attributes.RightSideStreetNameId
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Method
                        )
                    );
                    break;
            }
        }

        return changes;
    }

    private void MigrateRoadNodeIds(RoadSegmentFeatureCompareRecord record, ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        if (record.Attributes.StartNodeId is not null)
        {
            var startNode = context.FindNotRemovedRoadNode(record.Attributes.StartNodeId.Value);
            if (startNode is not null)
            {
                record.Attributes = record.Attributes with
                {
                    StartNodeId = startNode.Id
                };
            }
        }

        if (record.Attributes.EndNodeId is not null)
        {
            var endNode = context.FindNotRemovedRoadNode(record.Attributes.EndNodeId.Value);
            if (endNode is not null)
            {
                record.Attributes = record.Attributes with
                {
                    EndNodeId = endNode.Id
                };
            }
        }
    }

    private void AddRemovedRecordsToContext(List<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegmentRecords.Any(x => x.Id == extractFeature.Attributes.Id
                                                                              && !x.RecordType.Equals(RecordType.Added));
            if (!hasProcessedRoadSegment)
            {
                context.RoadSegmentRecords.Add(new RoadSegmentFeatureCompareRecord(
                    FeatureType.Extract,
                    extractFeature.RecordNumber,
                    extractFeature.Attributes,
                    extractFeature.Attributes.Id,
                    RecordType.Removed));
            }
        }
    }
}
