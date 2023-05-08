namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Uploads;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Point = NetTopologySuite.Geometries.Point;

internal class RoadNodeFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadNodeFeatureCompareAttributes>
{
    private record Record(Feature Feature, RecordType RecordType, int Id);
    
    public RoadNodeFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var featureReader = new VersionedFeatureReader<Feature>(
            new ExtractsFeatureReader(Encoding),
            new UploadsFeatureReader(Encoding)
        );

        var dbfFileName = GetDbfFileName(featureType, fileName);

        var features = featureReader.Read(entries, dbfFileName);

        var shpFileName = GetShpFileName(featureType, fileName);
        var shpEntry = entries.SingleOrDefault(x => x.Name.Equals(shpFileName, StringComparison.InvariantCultureIgnoreCase));
        if (shpEntry is null)
        {
            throw new FileNotFoundException($"File '{shpFileName}' was not found in zip archive", shpFileName);
        }
        
        var shpReader = new ZipArchiveShapeFileReader();
        foreach (var (geometry, recordNumber) in shpReader.Read(shpEntry))
        {
            var feature = features.Single(x => x.RecordNumber.Equals(recordNumber));
            feature.Attributes.Geometry = (Point)geometry;
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

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, "WEGKNOOP");

        var batchCount = 2;

        var processedLeveringRecords = await Task.WhenAll(
            leveringFeatures.SplitIntoBatches(batchCount)
                .Select(leveringFeaturesBatch =>
                {
                    return Task.Run(() => ProcessLeveringRecords(leveringFeaturesBatch, extractFeatures, cancellationToken), cancellationToken);
                }));
        var processedRecords = processedLeveringRecords.SelectMany(x => x).ToList();

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = processedRecords.Any(x => x.Id == extractFeature.Attributes.WK_OIDN);
            if (!hasProcessedRoadSegment)
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed, extractFeature.Attributes.WK_OIDN));
            }
        }

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadNode(
                            record.Feature.RecordNumber,
                            new RoadNodeId(record.Id),
                            RoadNodeType.ByIdentifier[record.Feature.Attributes.TYPE]
                        ).WithGeometry(record.Feature.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadNode(
                            record.Feature.RecordNumber,
                            new RoadNodeId(record.Id),
                            RoadNodeType.ByIdentifier[record.Feature.Attributes.TYPE]
                        ).WithGeometry(record.Feature.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadNode(
                            record.Feature.RecordNumber,
                            new RoadNodeId(record.Id)
                        )
                    );
                    break;
            }
        }

        return changes;
    }


    private List<Record> ProcessLeveringRecords(ICollection<Feature> leveringFeatures, ICollection<Feature> extractFeatures, CancellationToken cancellationToken)
    {
        var clusterTolerance = 0.05; // cfr WVB in GRB

        var processedRecords = new List<Record>();

        foreach (var leveringFeature in leveringFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = leveringFeature.Attributes.Geometry.Buffer(clusterTolerance);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry.Envelope) && x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var nonIntersectingGeometries = intersectingGeometries.FindAll(extractFeature =>
                    extractFeature.Attributes.TYPE == leveringFeature.Attributes.TYPE
                );
                int idValue;
                if (nonIntersectingGeometries.Any())
                {
                    idValue = nonIntersectingGeometries.First().Attributes.WK_OIDN;

                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical, idValue));
                }
                else
                {
                    idValue = intersectingGeometries.First().Attributes.WK_OIDN;

                    processedRecords.Add(new Record(leveringFeature, RecordType.Modified, idValue));
                }
            }
            else
            {
                processedRecords.Add(new Record(leveringFeature, RecordType.Added, leveringFeature.Attributes.WK_OIDN));
            }
        }

        return processedRecords;
    }

    private sealed class ExtractsFeatureReader : FeatureReader<RoadNodeDbaseRecord, Feature>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadNodeDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadNodeDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                TYPE = dbaseRecord.TYPE.Value,
                WK_OIDN = dbaseRecord.WK_OIDN.Value
            });
        }
    }

    private sealed class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord, Feature>
    {
        public UploadsFeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                TYPE = dbaseRecord.TYPE.Value,
                WK_OIDN = dbaseRecord.WK_OIDN.Value
            });
        }
    }
}
