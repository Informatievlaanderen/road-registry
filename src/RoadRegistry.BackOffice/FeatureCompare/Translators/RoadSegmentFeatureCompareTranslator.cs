namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Uploads;

internal class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    public RoadSegmentFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    private List<RoadSegmentRecord> ProcessLeveringRecords(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
    {
        var openGisGeometryType = OgcGeometryType.LineString;
        var clusterTolerance = 0.10; // cfr WVB in GRB = 0,15
        var criticalOverlapPercentage = 70.0;
        var buffersize = 1.0;

        var processedRecords = new List<RoadSegmentRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(buffersize);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry.Envelope) && x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var overlappingGeometries = intersectingGeometries.FindAll(f => changeFeature.Attributes.Geometry.OverlapsWith(f.Attributes.Geometry, criticalOverlapPercentage, openGisGeometryType, buffersize));
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
                            changeFeature.Attributes.Id = identicalFeatures.First().Attributes.Id;

                            processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes, RecordType.Identical)
                            {
                                CompareIdn = compareIdn
                            });
                        }
                        else
                        {
                            var compareIdn = changeFeature.Attributes.Id.ToString();
                            changeFeature.Attributes.Id = nonCriticalAttributesUnchanged.First().Attributes.Id;

                            //update because geometries differ (slightly)
                            processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes, RecordType.Modified)
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
                        changeFeature.Attributes.Id = overlappingGeometries.First().Attributes.Id;

                        processedRecords.Add(new RoadSegmentRecord(changeFeature.RecordNumber, changeFeature.Attributes, RecordType.Modified)
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

        return processedRecords;
    }

    protected override List<Feature<RoadSegmentFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new RoadSegmentFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    public override async Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, changeFeatures, integrationFeatures) = ReadExtractAndLeveringAndIntegrationFeatures(entries, "WEGSEGMENT");

        context.RoadSegments.AddRange(integrationFeatures.Select(feature => new RoadSegmentRecord(feature.RecordNumber, feature.Attributes, RecordType.Identical)));

        var batchCount = 2;

        if (changeFeatures.Any())
        {
            var processedLeveringRecords = await Task.WhenAll(
                changeFeatures.SplitIntoBatches(batchCount)
                    .Select(changeFeaturesBatch => { return Task.Run(() => ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, cancellationToken), cancellationToken); }));
            context.RoadSegments.AddRange(processedLeveringRecords.SelectMany(x => x));

            var rootNumber = Convert.ToInt32(changeFeatures.Max(x => x.Attributes.Id)) + 1;
            foreach (var record in context.RoadSegments.Where(x => x.RecordType.Equals(RecordType.Added)))
            {
                record.TempId = rootNumber++;
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

        foreach (var record in context.RoadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.IdenticalIdentifier:
                    changes = changes.AppendProvisionalChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.Attributes.Id),
                            new RoadNodeId(record.Attributes.StartNodeId),
                            new RoadNodeId(record.Attributes.EndNodeId),
                            new OrganizationId(record.Attributes.MaintenanceAuthority),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.Method],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.Morphology],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.Status],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.Category],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.AccessRestriction],
                            CrabStreetnameId.FromValue(record.Attributes.LeftStreetNameId),
                            CrabStreetnameId.FromValue(record.Attributes.RightStreetNameId)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.Attributes.Id),
                            new RoadNodeId(record.Attributes.StartNodeId),
                            new RoadNodeId(record.Attributes.EndNodeId),
                            new OrganizationId(record.Attributes.MaintenanceAuthority),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.Method],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.Morphology],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.Status],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.Category],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.AccessRestriction],
                            CrabStreetnameId.FromValue(record.Attributes.LeftStreetNameId),
                            CrabStreetnameId.FromValue(record.Attributes.RightStreetNameId)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.GetNewOrOriginalId()),
                            new RoadNodeId(record.Attributes.StartNodeId),
                            new RoadNodeId(record.Attributes.EndNodeId),
                            new OrganizationId(record.Attributes.MaintenanceAuthority),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.Method],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.Morphology],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.Status],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.Category],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.AccessRestriction],
                            CrabStreetnameId.FromValue(record.Attributes.LeftStreetNameId),
                            CrabStreetnameId.FromValue(record.Attributes.RightStreetNameId)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.Attributes.Id)
                        )
                    );
                    break;
            }
        }

        return changes;
    }
}
