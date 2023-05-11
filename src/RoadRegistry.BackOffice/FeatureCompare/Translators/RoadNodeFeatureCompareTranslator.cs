namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

internal class RoadNodeFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadNodeFeatureCompareAttributes>
{
    public RoadNodeFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    private List<Record> ProcessLeveringRecords(ICollection<Feature<RoadNodeFeatureCompareAttributes>> leveringFeatures, ICollection<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
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
                    extractFeature.Attributes.Type == leveringFeature.Attributes.Type
                );
                int idValue;
                if (nonIntersectingGeometries.Any())
                {
                    idValue = nonIntersectingGeometries.First().Attributes.Id;

                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical, idValue));
                }
                else
                {
                    idValue = intersectingGeometries.First().Attributes.Id;

                    processedRecords.Add(new Record(leveringFeature, RecordType.Modified, idValue));
                }
            }
            else
            {
                processedRecords.Add(new Record(leveringFeature, RecordType.Added, leveringFeature.Attributes.Id));
            }
        }

        return processedRecords;
    }

    protected override List<Feature<RoadNodeFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new RoadNodeFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    public override async Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, "WEGKNOOP");

        var batchCount = 2;

        var processedLeveringRecords = await Task.WhenAll(
            leveringFeatures.SplitIntoBatches(batchCount)
                .Select(leveringFeaturesBatch => { return Task.Run(() => ProcessLeveringRecords(leveringFeaturesBatch, extractFeatures, cancellationToken), cancellationToken); }));
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
                            new RoadNodeId(record.Id),
                            RoadNodeType.ByIdentifier[record.Feature.Attributes.Type]
                        ).WithGeometry(record.Feature.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadNode(
                            record.Feature.RecordNumber,
                            new RoadNodeId(record.Id),
                            RoadNodeType.ByIdentifier[record.Feature.Attributes.Type]
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

    private record Record(Feature<RoadNodeFeatureCompareAttributes> Feature, RecordType RecordType, int Id);
}
