namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Uploads;

internal class RoadNodeFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    private List<Record> ProcessLeveringRecords(ICollection<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
    {
        var clusterTolerance = 0.05; // cfr WVB in GRB

        var processedRecords = new List<Record>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(clusterTolerance);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry.Envelope) && x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var nonIntersectingGeometries = intersectingGeometries.FindAll(extractFeature =>
                    extractFeature.Attributes.Type == changeFeature.Attributes.Type
                );
                RoadNodeId idValue;
                if (nonIntersectingGeometries.Any())
                {
                    idValue = nonIntersectingGeometries.First().Attributes.Id;

                    processedRecords.Add(new Record(changeFeature, RecordType.Identical, idValue));
                }
                else
                {
                    idValue = intersectingGeometries.First().Attributes.Id;

                    processedRecords.Add(new Record(changeFeature, RecordType.Modified, idValue));
                }
            }
            else
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added, changeFeature.Attributes.Id));
            }
        }

        return processedRecords;
    }

    protected override (List<Feature<RoadNodeFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var featureReader = new RoadNodeFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(archive, featureType, fileName, context);
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, ExtractFileName.Wegknoop, context);

        var batchCount = 2;

        var processedLeveringRecords = await Task.WhenAll(
            changeFeatures.SplitIntoBatches(batchCount)
                .Select(changeFeaturesBatch => { return Task.Run(() => ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, cancellationToken), cancellationToken); }));
        var processedRecords = processedLeveringRecords.SelectMany(x => x).ToList();

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = processedRecords.Any(x => x.Id == extractFeature.Attributes.Id);
            if (!hasProcessedRoadSegment)
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed, extractFeature.Attributes.Id));
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
                            record.Id,
                            record.Feature.Attributes.Type
                        ).WithGeometry(record.Feature.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadNode(
                            record.Feature.RecordNumber,
                            record.Id,
                            record.Feature.Attributes.Type
                        ).WithGeometry(record.Feature.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadNode(
                            record.Feature.RecordNumber,
                            record.Id
                        )
                    );
                    break;
            }
        }

        return (changes, problems);
    }

    private sealed record Record(Feature<RoadNodeFeatureCompareAttributes> Feature, RecordType RecordType, RoadNodeId Id);
}
