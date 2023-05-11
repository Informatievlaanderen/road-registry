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

    private List<RoadSegmentRecord> ProcessLeveringRecords(ICollection<Feature<RoadSegmentFeatureCompareAttributes>> leveringFeatures, ICollection<Feature<RoadSegmentFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
    {
        var openGisGeometryType = OgcGeometryType.LineString;
        var clusterTolerance = 0.10; // cfr WVB in GRB = 0,15
        var criticalOverlapPercentage = 70.0;
        var buffersize = 1.0;

        var processedRecords = new List<RoadSegmentRecord>();

        foreach (var leveringFeature in leveringFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = leveringFeature.Attributes.Geometry.Buffer(buffersize);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry.Envelope) && x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var overlappingGeometries = intersectingGeometries.FindAll(f => leveringFeature.Attributes.Geometry.OverlapsWith(f.Attributes.Geometry, criticalOverlapPercentage, openGisGeometryType, buffersize));
                if (overlappingGeometries.Any())
                {
                    // Test op verschillen in niet kenmerkende attributen
                    var nonCriticalAttributesUnchanged = overlappingGeometries.FindAll(extractFeature =>
                        leveringFeature.Attributes.Status == extractFeature.Attributes.Status &&
                        leveringFeature.Attributes.Category == extractFeature.Attributes.Category &&
                        leveringFeature.Attributes.LeftStreetNameId == extractFeature.Attributes.LeftStreetNameId &&
                        leveringFeature.Attributes.RightStreetNameId == extractFeature.Attributes.RightStreetNameId &&
                        leveringFeature.Attributes.MaintenanceAuthority == extractFeature.Attributes.MaintenanceAuthority &&
                        leveringFeature.Attributes.Method == extractFeature.Attributes.Method &&
                        leveringFeature.Attributes.StartNodeId == extractFeature.Attributes.StartNodeId &&
                        leveringFeature.Attributes.EndNodeId == extractFeature.Attributes.EndNodeId &&
                        leveringFeature.Attributes.AccessRestriction == extractFeature.Attributes.AccessRestriction &&
                        leveringFeature.Attributes.Morphology == extractFeature.Attributes.Morphology
                    );
                    if (nonCriticalAttributesUnchanged.Any())
                    {
                        var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature => leveringFeature.Attributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, clusterTolerance));
                        if (identicalFeatures.Any())
                        {
                            var compareIdn = leveringFeature.Attributes.Id.ToString();
                            leveringFeature.Attributes.Id = identicalFeatures.First().Attributes.Id;

                            processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Identical)
                            {
                                CompareIdn = compareIdn
                            });
                        }
                        else
                        {
                            var compareIdn = leveringFeature.Attributes.Id.ToString();
                            leveringFeature.Attributes.Id = nonCriticalAttributesUnchanged.First().Attributes.Id;

                            //update because geometries differ (slightly)
                            processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Modified)
                            {
                                CompareIdn = compareIdn,
                                GeometryChanged = true
                            });
                        }
                    }
                    else
                    {
                        //no features with with unchanged non-critical attributes in criticalAttributesUnchanged
                        var identicalGeometries = overlappingGeometries.FindAll(f => leveringFeature.Attributes.Geometry.IsReasonablyEqualTo(f.Attributes.Geometry, clusterTolerance));

                        var compareIdn = leveringFeature.Attributes.Id.ToString();
                        leveringFeature.Attributes.Id = overlappingGeometries.First().Attributes.Id;

                        processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Modified)
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

                    processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Added)
                    {
                        CompareIdn = compareIdn.ToString()
                    });
                }
            }
            else
            {
                processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Added));
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

        var (extractFeatures, leveringFeatures, integrationFeatures) = ReadExtractAndLeveringAndIntegrationFeatures(entries, "WEGSEGMENT");

        context.RoadSegments.AddRange(integrationFeatures.Select(feature => new RoadSegmentRecord(feature.RecordNumber, feature.Attributes, RecordType.Identical)));

        var batchCount = 2;

        if (leveringFeatures.Any())
        {
            var processedLeveringRecords = await Task.WhenAll(
                leveringFeatures.SplitIntoBatches(batchCount)
                    .Select(leveringFeaturesBatch => { return Task.Run(() => ProcessLeveringRecords(leveringFeaturesBatch, extractFeatures, cancellationToken), cancellationToken); }));
            context.RoadSegments.AddRange(processedLeveringRecords.SelectMany(x => x));

            var rootNumber = Convert.ToInt32(leveringFeatures.Max(x => x.Attributes.Id)) + 1;
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
