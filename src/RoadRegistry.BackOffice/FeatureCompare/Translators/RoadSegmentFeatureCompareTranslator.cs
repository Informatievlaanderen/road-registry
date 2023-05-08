namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Uploads;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class RoadSegmentFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadSegmentFeatureCompareAttributes>
{
    private const int AddedEventIdn = -1;

    public RoadSegmentFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var featureReader = new VersionedFeatureReader<Feature>(
            new ExtractsFeatureReader(Encoding),
            new UploadsV2FeatureReader(Encoding)
        );

        var dbfFileName = GetDbfFileName(featureType, fileName);

        var features = featureReader.Read(entries, dbfFileName);

        var shpFileName = GetShpFileName(featureType, fileName);
        var shpEntry = entries.Single(x => x.Name.Equals(shpFileName, StringComparison.InvariantCultureIgnoreCase));
        if (shpEntry is null)
        {
            throw new FileNotFoundException($"File '{shpFileName}' was not found in zip archive", shpFileName);
        }

        var shpReader = new ZipArchiveShapeFileReader(Encoding);
        foreach (var (geometry, recordNumber) in shpReader.Read(shpEntry))
        {
            var multiLineString = geometry as MultiLineString;
            if (multiLineString is null)
            {
                var lineString = (LineString)geometry;
                multiLineString = new MultiLineString(new[] { lineString }, lineString.Factory)
                {
                    SRID = lineString.SRID
                };
            }

            var feature = features.Single(x => x.RecordNumber.Equals(recordNumber));
            feature.Attributes.Geometry = multiLineString;
        }

        var featuresWithoutGeometry = features.Where(x => x.Attributes.Geometry is null).ToArray();
        if (featuresWithoutGeometry.Any())
        {
            throw new InvalidOperationException($"{featuresWithoutGeometry.Length} {fileName} records have no geometry");
        }

        return features;
    }

    public override async Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, "WEGSEGMENT");

        var batchCount = 2;

        if (leveringFeatures.Any())
        {
            var processedLeveringRecords = await Task.WhenAll(
            leveringFeatures.SplitIntoBatches(batchCount)
                .Select(leveringFeaturesBatch =>
                {
                    return Task.Run(() => ProcessLeveringRecords(leveringFeaturesBatch, extractFeatures, cancellationToken), cancellationToken);
                }));
            context.RoadSegments.AddRange(processedLeveringRecords.SelectMany(x => x));

            var rootNumber = Convert.ToInt32(leveringFeatures.Max(x => x.Attributes.WS_OIDN)) + 1;
            foreach (var record in context.RoadSegments.Where(x => x.RecordType.Equals(RecordType.Added) && x.EventIdn == AddedEventIdn))
            {
                record.EventIdn = rootNumber++;
            }
        }

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadSegments.Any(x => x.Id == extractFeature.Attributes.WS_OIDN
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
                            new RoadSegmentId(record.Attributes.WS_OIDN),
                            new RoadNodeId(record.Attributes.B_WK_OIDN),
                            new RoadNodeId(record.Attributes.E_WK_OIDN),
                            new OrganizationId(record.Attributes.BEHEER),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.METHODE],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.MORF],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.STATUS],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.WEGCAT],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.TGBEP],
                            CrabStreetnameId.FromValue(record.Attributes.LSTRNMID),
                            CrabStreetnameId.FromValue(record.Attributes.RSTRNMID)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.Attributes.WS_OIDN),
                            new RoadNodeId(record.Attributes.B_WK_OIDN),
                            new RoadNodeId(record.Attributes.E_WK_OIDN),
                            new OrganizationId(record.Attributes.BEHEER),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.METHODE],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.MORF],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.STATUS],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.WEGCAT],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.TGBEP],
                            CrabStreetnameId.FromValue(record.Attributes.LSTRNMID),
                            CrabStreetnameId.FromValue(record.Attributes.RSTRNMID)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.EventIdn != 0 ? record.EventIdn : record.Attributes.WS_OIDN),
                            new RoadNodeId(record.Attributes.B_WK_OIDN),
                            new RoadNodeId(record.Attributes.E_WK_OIDN),
                            new OrganizationId(record.Attributes.BEHEER),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[record.Attributes.METHODE],
                            RoadSegmentMorphology.ByIdentifier[record.Attributes.MORF],
                            RoadSegmentStatus.ByIdentifier[record.Attributes.STATUS],
                            RoadSegmentCategory.ByIdentifier[record.Attributes.WEGCAT],
                            RoadSegmentAccessRestriction.ByIdentifier[record.Attributes.TGBEP],
                            CrabStreetnameId.FromValue(record.Attributes.LSTRNMID),
                            CrabStreetnameId.FromValue(record.Attributes.RSTRNMID)
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegment(
                            record.RecordNumber,
                            new RoadSegmentId(record.Attributes.WS_OIDN)
                        )
                    );
                    break;
            }
        }

        return changes;
    }

    private List<RoadSegmentRecord> ProcessLeveringRecords(ICollection<Feature> leveringFeatures, ICollection<Feature> extractFeatures, CancellationToken cancellationToken)
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
                        leveringFeature.Attributes.STATUS == extractFeature.Attributes.STATUS &&
                        leveringFeature.Attributes.WEGCAT == extractFeature.Attributes.WEGCAT &&
                        leveringFeature.Attributes.LSTRNMID == extractFeature.Attributes.LSTRNMID &&
                        leveringFeature.Attributes.RSTRNMID == extractFeature.Attributes.RSTRNMID &&
                        leveringFeature.Attributes.BEHEER == extractFeature.Attributes.BEHEER &&
                        leveringFeature.Attributes.METHODE == extractFeature.Attributes.METHODE &&
                        leveringFeature.Attributes.B_WK_OIDN == extractFeature.Attributes.B_WK_OIDN &&
                        leveringFeature.Attributes.E_WK_OIDN == extractFeature.Attributes.E_WK_OIDN &&
                        leveringFeature.Attributes.TGBEP == extractFeature.Attributes.TGBEP &&
                        leveringFeature.Attributes.MORF == extractFeature.Attributes.MORF
                    );
                    if (nonCriticalAttributesUnchanged.Any())
                    {
                        var identicalFeatures = nonCriticalAttributesUnchanged.FindAll(extractFeature => leveringFeature.Attributes.Geometry.IsReasonablyEqualTo(extractFeature.Attributes.Geometry, clusterTolerance));
                        if (identicalFeatures.Any())
                        {
                            var compareIdn = leveringFeature.Attributes.WS_OIDN.ToString();
                            leveringFeature.Attributes.WS_OIDN = identicalFeatures.First().Attributes.WS_OIDN;

                            processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Identical)
                            {
                                CompareIdn = compareIdn
                            });
                        }
                        else
                        {
                            var compareIdn = leveringFeature.Attributes.WS_OIDN.ToString();
                            leveringFeature.Attributes.WS_OIDN = nonCriticalAttributesUnchanged.First().Attributes.WS_OIDN;

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

                        var compareIdn = leveringFeature.Attributes.WS_OIDN.ToString();
                        leveringFeature.Attributes.WS_OIDN = overlappingGeometries.First().Attributes.WS_OIDN;

                        processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Modified)
                        {
                            CompareIdn = compareIdn,
                            GeometryChanged = !identicalGeometries.Any()
                        });
                    }
                }
                else
                {
                    var compareIdn = string.Empty;
                    foreach (var f in intersectingGeometries)
                    {
                        compareIdn += f.Attributes.WS_OIDN + ";";
                    }

                    processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Added)
                    {
                        CompareIdn = compareIdn,
                        EventIdn = AddedEventIdn
                    });
                }
            }
            else
            {
                processedRecords.Add(new RoadSegmentRecord(leveringFeature.RecordNumber, leveringFeature.Attributes, RecordType.Added)
                {
                    EventIdn = AddedEventIdn
                });
            }
        }

        return processedRecords;
    }

    private class ExtractsFeatureReader : FeatureReader<RoadSegmentDbaseRecord, Feature>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                B_WK_OIDN = dbaseRecord.B_WK_OIDN.HasValue ? dbaseRecord.B_WK_OIDN.Value : 0,
                BEHEER = dbaseRecord.BEHEER.Value,
                E_WK_OIDN = dbaseRecord.E_WK_OIDN.HasValue ? dbaseRecord.E_WK_OIDN.Value : 0,
                LSTRNMID = dbaseRecord.LSTRNMID.Value,
                METHODE = dbaseRecord.METHODE.Value,
                MORF = dbaseRecord.MORF.Value,
                RSTRNMID = dbaseRecord.RSTRNMID.Value,
                STATUS = dbaseRecord.STATUS.Value,
                TGBEP = dbaseRecord.TGBEP.Value,
                WEGCAT = dbaseRecord.WEGCAT.Value,
                WS_OIDN = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private class UploadsV2FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord, Feature>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                B_WK_OIDN = dbaseRecord.B_WK_OIDN.HasValue ? dbaseRecord.B_WK_OIDN.Value : 0,
                BEHEER = dbaseRecord.BEHEER.Value,
                E_WK_OIDN = dbaseRecord.E_WK_OIDN.HasValue ? dbaseRecord.E_WK_OIDN.Value : 0,
                LSTRNMID = dbaseRecord.LSTRNMID.Value,
                METHODE = dbaseRecord.METHODE.Value,
                MORF = dbaseRecord.MORF.Value,
                RSTRNMID = dbaseRecord.RSTRNMID.Value,
                STATUS = dbaseRecord.STATUS.Value,
                TGBEP = dbaseRecord.TGBEP.Value,
                WEGCAT = dbaseRecord.WEGCAT.Value,
                WS_OIDN = dbaseRecord.WS_OIDN.Value
            });
        }
    }
}
