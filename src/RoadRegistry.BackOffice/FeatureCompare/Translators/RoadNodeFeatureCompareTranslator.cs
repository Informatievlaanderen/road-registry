namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.Diagnostics;
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

    private List<RoadNodeFeatureCompareRecord> ProcessLeveringRecords(ICollection<Feature<RoadNodeFeatureCompareAttributes>> changeFeatures, ICollection<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, CancellationToken cancellationToken)
    {
        var clusterTolerance = 0.05; // cfr WVB in GRB

        var processedRecords = new List<RoadNodeFeatureCompareRecord>();

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedGeometry = changeFeature.Attributes.Geometry.Buffer(clusterTolerance);
            var intersectingGeometries = extractFeatures
                .Where(x => x.Attributes.Geometry.Intersects(bufferedGeometry))
                .ToList();

            if (intersectingGeometries.Any())
            {
                var intersectingGeometriesWithSameType = intersectingGeometries.FindAll(extractFeature =>
                    extractFeature.Attributes.Type == changeFeature.Attributes.Type
                );
                if (intersectingGeometriesWithSameType.Any())
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(changeFeature.RecordNumber, changeFeature.Attributes, intersectingGeometriesWithSameType.First().Attributes.Id, RecordType.Identical));
                }
                else
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(changeFeature.RecordNumber, changeFeature.Attributes, intersectingGeometries.First().Attributes.Id, RecordType.Modified));
                }
            }
            else
            {
                var extractFeature = extractFeatures.FirstOrDefault(x => x.Attributes.Id == changeFeature.Attributes.Id);
                if (extractFeature is not null)
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(extractFeature.RecordNumber, extractFeature.Attributes, extractFeature.Attributes.Id, RecordType.Removed));
                }
                
                processedRecords.Add(new RoadNodeFeatureCompareRecord(changeFeature.RecordNumber, changeFeature.Attributes, changeFeature.Attributes.Id, RecordType.Added));
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

        var batchCount = Debugger.IsAttached ? 1 : 2;

        var processedLeveringRecords = await Task.WhenAll(
            changeFeatures.SplitIntoBatches(batchCount)
                .Select(changeFeaturesBatch => Task.Run(() =>
                    ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, cancellationToken), cancellationToken)
                ));
        context.RoadNodeRecords.AddRange(processedLeveringRecords.SelectMany(x => x));

        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadSegment = context.RoadNodeRecords.Any(x => x.Id == extractFeature.Attributes.Id);
            if (!hasProcessedRoadSegment)
            {
                context.RoadNodeRecords.Add(new RoadNodeFeatureCompareRecord(extractFeature.RecordNumber, extractFeature.Attributes, extractFeature.Attributes.Id, RecordType.Removed));
            }
        }

        foreach (var record in context.RoadNodeRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadNode(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Type
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.ModifiedIdentifier:
                    changes = changes.AppendChange(
                        new ModifyRoadNode(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Type
                        ).WithGeometry(record.Attributes.Geometry)
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadNode(
                            record.RecordNumber,
                            record.Id
                        )
                    );
                    break;
            }
        }

        return (changes, problems);
    }
}
