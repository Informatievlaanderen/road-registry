namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using NetTopologySuite.Geometries;
using Uploads;

internal class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    private (List<RoadSegmentRecord>, ZipArchiveProblems) ProcessLeveringRecords(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        var clusterTolerance = 0.10; // cfr WVB in GRB = 0,15
        var bufferSize = 1.0;

        var processedRecords = new List<RoadSegmentRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(bufferSize);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry.Envelope) && x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var overlappingGeometries = intersectingGeometries.FindAll(f => changeFeature.Attributes.Geometry.RoadSegmentOverlapsWith(f.Attributes.Geometry));
                if (overlappingGeometries.Any())
                {
                    // Test op verschillen in niet kenmerkende attributen
                    var nonCriticalAttributesUnchanged = overlappingGeometries.FindAll(extractFeature =>
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
                        var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature => changeFeature.Attributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, clusterTolerance));
                        if (identicalFeatures.Any())
                        {
                            var compareIdn = changeFeature.Attributes.Id.ToString();

                            processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes with
                            {
                                Id = identicalFeatures.First().Attributes.Id
                            }, RecordType.Identical)
                            {
                                CompareIdn = compareIdn
                            });
                        }
                        else
                        {
                            var compareIdn = changeFeature.Attributes.Id.ToString();

                            //update because geometries differ (slightly)
                            processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes with
                            {
                                Id = nonCriticalAttributesUnchanged.First().Attributes.Id
                            }, RecordType.Modified)
                            {
                                CompareIdn = compareIdn,
                                GeometryChanged = true
                            });
                        }
                    }
                    else
                    {
                        //no features with with unchanged non-critical attributes in criticalAttributesUnchanged
                        var identicalGeometries = overlappingGeometries.FindAll(f => changeFeature.Attributes.Geometry.IsReasonablyEqualTo(f.Attributes.Geometry, clusterTolerance));

                        var compareIdn = changeFeature.Attributes.Id.ToString();

                        processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes with
                        {
                            Id = overlappingGeometries.First().Attributes.Id
                        }, RecordType.Modified)
                        {
                            CompareIdn = compareIdn,
                            GeometryChanged = !identicalGeometries.Any()
                        });
                    }
                }
                else
                {
                    var compareIdn = new StringBuilder();
                    foreach (var f in intersectingGeometries)
                    {
                        compareIdn.Append(f.Attributes.Id + ";");
                    }

                    processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes, RecordType.Added)
                    {
                        CompareIdn = compareIdn.ToString()
                    });
                }
            }
            else
            {
                processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes, RecordType.Added));
            }
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
        var (extractFeatures, changeFeatures, integrationFeatures, problems) = ReadExtractAndLeveringAndIntegrationFeatures(context.Archive, FileName, context);

        context.RoadSegments.AddRange(integrationFeatures.Select(feature => new RoadSegmentRecord(feature.RecordNumber, feature.Attributes, RecordType.Identical)
        {
            FeatureType = FeatureType.Integration
        }));

        const int batchCount = 2;

        if (changeFeatures.Any())
        {
            var processedLeveringRecords = await Task.WhenAll(
                changeFeatures.SplitIntoBatches(batchCount)
                    .Select(changeFeaturesBatch => { return Task.Run(() => ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, cancellationToken), cancellationToken); }));
            
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
                record.TempId = nextId;
                nextId = nextId.Next();
            }

            foreach (var record in processedLeveringRecords.SelectMany(x => x.Item1))
            {
                var existingRecord = context.RoadSegments.SingleOrDefault(x => x.GetNewOrOriginalId() == record.Id);
                if (existingRecord is not null)
                {
                    var recordContext = FileName.AtDbaseRecord(FeatureType.Change, record.RecordNumber);

                    problems += recordContext.RoadSegmentIsAlreadyProcessed(record.Id, existingRecord.Id);
                    continue;
                }

                context.RoadSegments.Add(record);
            }
        }

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegments.Any(x => x.Id == extractFeature.Attributes.Id
                                                                        && !x.RecordType.Equals(RecordType.Added));
            if (!hasProcessedRoadSegment)
            {
                context.RoadSegments.Add(new RoadSegmentRecord(extractFeature.RecordNumber, extractFeature.Attributes, RecordType.Removed));
            }
        }

        foreach (var record in context.RoadSegments.Where(x => x.FeatureType != FeatureType.Integration))
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    changes = changes.AppendProvisionalChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
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
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
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
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegment(
                            record.RecordNumber,
                            record.GetNewOrOriginalId(),
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
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegment(
                            record.RecordNumber,
                            record.Attributes.Id
                        )
                    );
                    break;
            }
        }

        return (changes, problems);
    }
}
