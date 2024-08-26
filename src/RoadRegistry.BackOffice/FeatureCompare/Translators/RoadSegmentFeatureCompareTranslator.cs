namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Core;
using Extracts;
using Extracts.Dbase.RoadSegments;
using NetTopologySuite.Geometries;
using Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uploads;
using AddRoadSegment = Uploads.AddRoadSegment;
using ModifyRoadSegment = Uploads.ModifyRoadSegment;
using RemoveOutlinedRoadSegment = Uploads.RemoveOutlinedRoadSegment;
using RemoveRoadSegment = Uploads.RemoveRoadSegment;

public class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    private readonly IOrganizationCache _organizationCache;
    private readonly IStreetNameCache _streetNameCache;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareTranslator(
        RoadSegmentFeatureCompareFeatureReader featureReader,
        IOrganizationCache organizationCache,
        IStreetNameCache streetNameCache)
        : base(featureReader)
    {
        _organizationCache = organizationCache.ThrowIfNull();
        _streetNameCache = streetNameCache.ThrowIfNull();
    }

    private async Task<(List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems)> ProcessLeveringRecords(
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures,
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures,
        RoadSegmentFeatureCompareStreetNameContext streetNameContext,
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

            var bufferedGeometry = changeFeatureAttributes.Geometry.Buffer(clusterTolerance);
            return extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
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

        StreetNameLocalId? CorrectStreetNameId(StreetNameLocalId? id)
        {
            if (id > 0)
            {
                if (streetNameContext.RemovedIds.Contains(id.Value))
                {
                    return StreetNameLocalId.NotApplicable;
                }

                if (streetNameContext.RenamedIds.TryGetValue(id.Value, out var renamedToId))
                {
                    return StreetNameLocalId.FromValue(renamedToId);
                }
            }

            return id;
        }

        RoadSegmentFeatureCompareAttributes CorrectStreetNameIds(RoadSegmentFeatureCompareAttributes changeFeatureAttributes)
        {
            return changeFeatureAttributes with
            {
                LeftStreetNameId = CorrectStreetNameId(changeFeatureAttributes.LeftStreetNameId),
                RightStreetNameId = CorrectStreetNameId(changeFeatureAttributes.RightStreetNameId)
            };
        }

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changeFeatureAttributes = changeFeature.Attributes;

            if (changeFeatureAttributes.Method != RoadSegmentGeometryDrawMethod.Outlined)
            {
                var startNodeFeature = FindRoadNodeByOriginalId(changeFeatureAttributes.StartNodeId);
                var endNodeFeature = FindRoadNodeByOriginalId(changeFeatureAttributes.EndNodeId);

                if (startNodeFeature is null || endNodeFeature is null)
                {
                    var recordContext = FileName
                        .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                        .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), changeFeature.Attributes.Id);

                    if (startNodeFeature is null)
                    {
                        problems += recordContext.BeginRoadNodeIdOutOfRange(changeFeatureAttributes.StartNodeId);
                    }

                    if (endNodeFeature is null)
                    {
                        problems += recordContext.EndRoadNodeIdOutOfRange(changeFeatureAttributes.EndNodeId);
                    }

                    continue;
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
                    changeFeatureAttributes.LeftStreetNameId == extractFeature.Attributes.LeftStreetNameId &&
                    changeFeatureAttributes.RightStreetNameId == extractFeature.Attributes.RightStreetNameId &&
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
                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes,
                            identicalFeatures.First().Attributes.Id,
                            RecordType.Identical));
                    }
                    else
                    {
                        //update because geometries differ (slightly)
                        var extractFeature = nonCriticalAttributesUnchanged.First();

                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeatureAttributes,
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
                    var extractFeature = matchingExtractFeatures.First();

                    processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeatureAttributes,
                        extractFeature.Attributes.Id,
                        RecordType.Modified)
                    {
                        GeometryChanged = !identicalGeometries.Any(),
                        PreviousGeometryDrawMethod = extractFeature.Attributes.Method,
                        CategoryModified = extractFeature.Attributes.Category != changeFeatureAttributes.Category
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
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, FileName, context);

        problems.ThrowIfError();

        context.RoadSegmentRecords.AddRange(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, feature.Attributes, feature.Attributes.Id, RecordType.Identical)
        ));

        if (changeFeatures.Any())
        {
            (changeFeatures, var maintenanceAuthorityProblems) = await ValidateMaintenanceAuthorityAndMapToInternalId(changeFeatures, cancellationToken);
            problems += maintenanceAuthorityProblems;

            var streetNameContext = await RoadSegmentFeatureCompareStreetNameContext.FromFeatures(changeFeatures, _streetNameCache, cancellationToken);
            var batchCount = Debugger.IsAttached ? 1 : 2;

            var processedLeveringRecords = await Task.WhenAll(
                changeFeatures.SplitIntoBatches(batchCount)
                    .Select(changeFeaturesBatch => ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, streetNameContext, context, cancellationToken))
            );

            foreach (var processedProblems in processedLeveringRecords.Select(x => x.Item2))
            {
                problems += processedProblems;
            }

            var maxId = integrationFeatures.Select(x => x.Attributes.Id)
                .Concat(extractFeatures.Select(x => x.Attributes.Id))
                .Concat(changeFeatures.Select(x => x.Attributes.Id))
                .Max();

            problems += AddProcessedRecordsToContext(maxId, processedLeveringRecords.SelectMany(x => x.Item1).ToList(), context, cancellationToken);
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
            var maintenanceAuthorityCode = changeFeature.Attributes.MaintenanceAuthority;

            var maintenanceAuthority = await _organizationCache.FindByIdOrOvoCodeAsync(maintenanceAuthorityCode, cancellationToken);
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
                            record.Attributes.StartNodeId,
                            record.Attributes.EndNodeId,
                            record.Attributes.MaintenanceAuthority,
                            record.Attributes.Method,
                            record.Attributes.Morphology,
                            record.Attributes.Status,
                            record.Attributes.Category,
                            record.Attributes.AccessRestriction,
                            record.Attributes.LeftStreetNameId,
                            record.Attributes.RightStreetNameId
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    var convertedFromOutlined = record.PreviousGeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                                                && record.Attributes.Method != record.PreviousGeometryDrawMethod;
                    var convertedToOutlined = record.Attributes.Method == RoadSegmentGeometryDrawMethod.Outlined
                                              && record.Attributes.Method != record.PreviousGeometryDrawMethod;
                    //TODO-rik Q naar Jelmer gestuurd: indien OK dan bij conversie van ingemeten->ingeschetst simpelweg het ingemeten wegsegment verwijderen en een nieuwe inschetste toevoegen
                    var modifyRoadSegment = new ModifyRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.StartNodeId,
                            record.Attributes.EndNodeId,
                            record.Attributes.MaintenanceAuthority,
                            record.Attributes.Method,
                            record.Attributes.Morphology,
                            record.Attributes.Status,
                            record.Attributes.Category,
                            record.Attributes.AccessRestriction,
                            record.Attributes.LeftStreetNameId,
                            record.Attributes.RightStreetNameId
                        )
                        .WithGeometry(record.Attributes.Geometry)
                        .WithCategoryModified(record.CategoryModified);
                    if (record.Id != record.Attributes.Id)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithOriginalId(record.Attributes.Id);
                    }
                    if (record.PreviousGeometryDrawMethod is not null)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithPreviousGeometryDrawMethod(record.PreviousGeometryDrawMethod);
                    }

                    changes = changes.AppendChange(modifyRoadSegment);

                    if (convertedFromOutlined)
                    {
                        changes = changes.AppendChange(
                            new RemoveOutlinedRoadSegment(
                                record.RecordNumber,
                                record.Id
                            )
                        );
                    }
                    else if (convertedToOutlined)
                    {
                        //TODO-rik is dit ook nodig om te doen? of iets generiekers zoals hierboven maar dan volgens de PreviousGeometryDrawMethod
                        // changes = changes.AppendChange(
                        //     new RemoveRoadSegment(
                        //         record.RecordNumber,
                        //         record.Id
                        //     )
                        // );
                    }

                    break;
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegment(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Id,
                            record.Attributes.StartNodeId,
                            record.Attributes.EndNodeId,
                            record.Attributes.MaintenanceAuthority,
                            record.Attributes.Method,
                            record.Attributes.Morphology,
                            record.Attributes.Status,
                            record.Attributes.Category,
                            record.Attributes.AccessRestriction,
                            record.Attributes.LeftStreetNameId,
                            record.Attributes.RightStreetNameId
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
        var startNode = context.FindNotRemovedRoadNode(record.Attributes.StartNodeId);
        if (startNode is not null)
        {
            record.Attributes = record.Attributes with
            {
                StartNodeId = startNode.Id
            };
        }

        var endNode = context.FindNotRemovedRoadNode(record.Attributes.EndNodeId);
        if (endNode is not null)
        {
            record.Attributes = record.Attributes with
            {
                EndNodeId = endNode.Id
            };
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
                context.RoadSegmentRecords.Add(new RoadSegmentFeatureCompareRecord(FeatureType.Extract, extractFeature.RecordNumber, extractFeature.Attributes, extractFeature.Attributes.Id, RecordType.Removed));
            }
        }
    }
}

public sealed class RoadSegmentFeatureCompareStreetNameContext
{
    public ICollection<int> RemovedIds { get; init; } = Array.Empty<int>();
    public IDictionary<int, int> RenamedIds { get; init; } = new Dictionary<int, int>();

    public static async Task<RoadSegmentFeatureCompareStreetNameContext> FromFeatures(
        ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures,
        IStreetNameCache streetNameCache,
        CancellationToken cancellationToken)
    {
        var usedStreetNameIds = changeFeatures
            .Where(x => x.Attributes.LeftStreetNameId > 0)
            .Select(x => x.Attributes.LeftStreetNameId)
            .Concat(changeFeatures
                .Where(x => x.Attributes.RightStreetNameId > 0)
                .Select(x => x.Attributes.RightStreetNameId))
            .Select(x => x.Value.ToInt32())
            .Distinct()
            .ToList();

        if (!usedStreetNameIds.Any())
        {
            return new RoadSegmentFeatureCompareStreetNameContext();
        }

        var removedStreetNameIds = (await streetNameCache.GetAsync(usedStreetNameIds, cancellationToken))
            .Where(x => x.IsRemoved)
            .Select(x => x.Id)
            .ToArray();
        var renamedStreetNameIds = await streetNameCache.GetRenamedIdsAsync(usedStreetNameIds, cancellationToken);

        return new RoadSegmentFeatureCompareStreetNameContext
        {
            RemovedIds = removedStreetNameIds,
            RenamedIds = renamedStreetNameIds
        };
    }
}
