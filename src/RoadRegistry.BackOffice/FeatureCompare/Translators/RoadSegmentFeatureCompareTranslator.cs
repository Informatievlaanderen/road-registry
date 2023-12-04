namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using Extracts;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;
using AddRoadSegment = Uploads.AddRoadSegment;
using ModifyRoadSegment = Uploads.ModifyRoadSegment;
using RemoveRoadSegment = Uploads.RemoveRoadSegment;

internal class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    private (List<RoadSegmentFeatureCompareRecord>, ZipArchiveProblems) ProcessLeveringRecords(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        var processedRecords = new List<RoadSegmentFeatureCompareRecord>();

        List<Feature<RoadSegmentFeatureCompareAttributes>> FindMatchingExtractFeatures(Feature<RoadSegmentFeatureCompareAttributes> changeFeature)
        {
            if (changeFeature.Attributes.Method == RoadSegmentGeometryDrawMethod.Outlined)
            {
                return extractFeatures
                    .Where(x => x.Attributes.Id == changeFeature.Attributes.Id)
                    .ToList();
            }

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(context.Tolerances.IntersectionBuffer);
            return extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
                .Where(x => changeFeature.Attributes.Geometry.RoadSegmentOverlapsWith(x.Attributes.Geometry))
                .ToList();
        }

        foreach (var changeFeature in changeFeatures)
        {
            //TODO-rik #797 use orgreader to find OrgId for MaintenanceAuthority
            cancellationToken.ThrowIfCancellationRequested();

            var matchingExtractFeatures = FindMatchingExtractFeatures(changeFeature);
            if (matchingExtractFeatures.Any())
            {
                // Test op verschillen in niet kenmerkende attributen
                var nonCriticalAttributesUnchanged = matchingExtractFeatures.FindAll(extractFeature =>
                    changeFeature.Attributes.Status == extractFeature.Attributes.Status &&
                    changeFeature.Attributes.Category == extractFeature.Attributes.Category &&
                    changeFeature.Attributes.LeftStreetNameId == extractFeature.Attributes.LeftStreetNameId &&
                    changeFeature.Attributes.RightStreetNameId == extractFeature.Attributes.RightStreetNameId &&
                    changeFeature.Attributes.MaintenanceAuthority == extractFeature.Attributes.MaintenanceAuthority &&
                    changeFeature.Attributes.Method == extractFeature.Attributes.Method &&
                    changeFeature.Attributes.StartNodeId == extractFeature.Attributes.StartNodeId &&
                    changeFeature.Attributes.EndNodeId == extractFeature.Attributes.EndNodeId &&
                    changeFeature.Attributes.AccessRestriction == extractFeature.Attributes.AccessRestriction &&
                    changeFeature.Attributes.Morphology == extractFeature.Attributes.Morphology
                );
                if (nonCriticalAttributesUnchanged.Any())
                {
                    var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature => changeFeature.Attributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, context.Tolerances.ClusterTolerance));
                    if (identicalFeatures.Any())
                    {
                        processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                            FeatureType.Change,
                            changeFeature.RecordNumber,
                            changeFeature.Attributes,
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
                            changeFeature.Attributes,
                            extractFeature.Attributes.Id,
                            RecordType.Modified)
                        {
                            GeometryChanged = true,
                            ConvertedFromOutlined = extractFeature.Attributes.Method == RoadSegmentGeometryDrawMethod.Outlined
                                                    && changeFeature.Attributes.Method != extractFeature.Attributes.Method
                        });
                    }
                }
                else
                {
                    //no features with with unchanged non-critical attributes in criticalAttributesUnchanged
                    var identicalGeometries = matchingExtractFeatures.FindAll(f => changeFeature.Attributes.Geometry.IsReasonablyEqualTo(f.Attributes.Geometry, context.Tolerances.ClusterTolerance));
                    var extractFeature = matchingExtractFeatures.First();

                    processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        extractFeature.Attributes.Id,
                        RecordType.Modified)
                    {
                        GeometryChanged = !identicalGeometries.Any(),
                        ConvertedFromOutlined = extractFeature.Attributes.Method == RoadSegmentGeometryDrawMethod.Outlined
                                                && changeFeature.Attributes.Method != extractFeature.Attributes.Method
                    });
                }
                continue;
            }

            processedRecords.Add(new RoadSegmentFeatureCompareRecord(
                FeatureType.Change,
                changeFeature.RecordNumber,
                changeFeature.Attributes,
                changeFeature.Attributes.Id,
                RecordType.Added));
        }

        return (processedRecords, problems);
    }

    protected override (List<Feature<RoadSegmentFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var featureReader = new RoadSegmentFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(archive, featureType, fileName, context);
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndChangeAndIntegrationFeatures(context.Archive, FileName, context);

        problems.ThrowIfError();

        context.RoadSegmentRecords.AddRange(integrationFeatures.Select(feature =>
            new RoadSegmentFeatureCompareRecord(FeatureType.Integration, feature.RecordNumber, feature.Attributes, feature.Attributes.Id, RecordType.Identical)
        ));

        var batchCount = Debugger.IsAttached ? 1 : 2;

        if (changeFeatures.Any())
        {
            var processedLeveringRecords = await Task.WhenAll(
                changeFeatures.SplitIntoBatches(batchCount)
                    .Select(changeFeaturesBatch => Task.Run(() =>
                        ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, context, cancellationToken), cancellationToken)
                    ));

            foreach (var processedProblems in processedLeveringRecords.Select(x => x.Item2))
            {
                problems += processedProblems;
            }

            var maxId = integrationFeatures.Select(x => x.Attributes.Id)
                .Concat(extractFeatures.Select(x => x.Attributes.Id))
                .Concat(changeFeatures.Select(x => x.Attributes.Id))
                .Max();

            var nextId = maxId.Next();
            foreach (var record in processedLeveringRecords
                         .SelectMany(x => x.Item1)
                         .Where(x => x.RecordType.Equals(RecordType.Added)))
            {
                record.Id = nextId;
                nextId = nextId.Next();
            }

            foreach (var record in processedLeveringRecords.SelectMany(x => x.Item1))
            {
                var existingRecords = context.RoadSegmentRecords
                    .Where(x => x.GetActualId() == record.Id)
                    .ToArray();

                if (existingRecords.Length > 1)
                {
                    var recordContext = FileName.AtDbaseRecord(FeatureType.Change, record.RecordNumber);

                    problems += recordContext.IdentifierNotUnique(record.Id, record.RecordNumber);
                    continue;
                }

                var existingRecord = existingRecords.SingleOrDefault();
                if (existingRecord is not null)
                {
                    var recordContext = FileName.AtDbaseRecord(FeatureType.Change, record.RecordNumber);

                    problems += recordContext.RoadSegmentIsAlreadyProcessed(record.GetOriginalId(), existingRecord.GetOriginalId());
                    continue;
                }

                MigrateRoadNodeIds(record, context);

                context.RoadSegmentRecords.Add(record);
            }
        }

        AddRemovedRecordsToContext(extractFeatures, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context.RoadSegmentRecords, cancellationToken);

        return (changes, problems);
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
                    changes = changes.AppendChange(
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
                        )
                        .WithGeometry(record.Attributes.Geometry)
                        .WithConvertedFromOutlined(record.ConvertedFromOutlined)
                    );

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
