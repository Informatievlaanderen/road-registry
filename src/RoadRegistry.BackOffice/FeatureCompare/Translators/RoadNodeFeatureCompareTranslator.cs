namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Extracts;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public class RoadNodeFeatureCompareTranslator : FeatureCompareTranslatorBase<RoadNodeFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.Wegknoop;

    public RoadNodeFeatureCompareTranslator(RoadNodeFeatureCompareFeatureReader featureReader)
        : base(featureReader)
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
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        intersectingGeometriesWithSameType.First().Attributes.Id,
                        RecordType.Identical));
                }
                else
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        changeFeature.RecordNumber,
                        changeFeature.Attributes,
                        intersectingGeometries.First().Attributes.Id,
                        RecordType.Modified));
                }
            }
            else
            {
                var extractFeature = extractFeatures.FirstOrDefault(x => x.Attributes.Id == changeFeature.Attributes.Id);
                if (extractFeature is not null)
                {
                    processedRecords.Add(new RoadNodeFeatureCompareRecord(
                        FeatureType.Change,
                        extractFeature.RecordNumber,
                        extractFeature.Attributes,
                        extractFeature.Attributes.Id,
                        RecordType.Removed));
                }

                processedRecords.Add(new RoadNodeFeatureCompareRecord(
                    FeatureType.Change,
                    changeFeature.RecordNumber,
                    changeFeature.Attributes,
                    changeFeature.Attributes.Id,
                    RecordType.Added));
            }
        }

        return processedRecords;
    }
    
    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, ExtractFileName.Wegknoop, context);

        problems.ThrowIfError();

        if (changeFeatures.Any())
        {
            var batchCount = Debugger.IsAttached ? 1 : 2;

            var processedLeveringRecords = await Task.WhenAll(
                changeFeatures.SplitIntoBatches(batchCount)
                    .Select(changeFeaturesBatch => Task.Run(() =>
                        ProcessLeveringRecords(changeFeaturesBatch, extractFeatures, cancellationToken), cancellationToken)
                    ));

            problems += AddProcessedRecordsToContext(processedLeveringRecords.SelectMany(x => x).ToList(), context, cancellationToken);
        }

        AddRemovedRecordsToContext(extractFeatures, context, cancellationToken);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, context.RoadNodeRecords, cancellationToken);

        return (changes, problems);
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<RoadNodeFeatureCompareRecord> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadNode(
                            record.RecordNumber,
                            record.Id,
                            record.Attributes.Id,
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

        return changes;
    }

    private ZipArchiveProblems AddProcessedRecordsToContext(ICollection<RoadNodeFeatureCompareRecord> processedRecords, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        foreach (var record in processedRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (record.RecordType != RecordType.Removed)
            {
                var existingRecords = context.RoadNodeRecords
                    .Where(x => x.RecordType != RecordType.Removed && x.GetActualId() == record.GetActualId())
                    .ToArray();

                if (existingRecords.Length > 1)
                {
                    var recordContext = FileName.AtDbaseRecord(FeatureType.Change, record.RecordNumber);

                    problems += recordContext.IdentifierNotUnique(record.GetActualId(), record.RecordNumber);
                    continue;
                }

                var existingRecord = existingRecords.SingleOrDefault();
                if (existingRecord is not null)
                {
                    var recordContext = FileName.AtDbaseRecord(FeatureType.Change, record.RecordNumber);

                    problems += recordContext.RoadNodeIsAlreadyProcessed(record.GetOriginalId(), existingRecord.GetOriginalId());
                    continue;
                }
            }

            context.RoadNodeRecords.Add(record);
        }

        return problems;
    }

    private void AddRemovedRecordsToContext(List<Feature<RoadNodeFeatureCompareAttributes>> extractFeatures, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var extractFeature in extractFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasProcessedRoadNode = context.RoadNodeRecords.Any(x => x.Id == extractFeature.Attributes.Id
                                                                        && !x.RecordType.Equals(RecordType.Added));
            if (!hasProcessedRoadNode)
            {
                context.RoadNodeRecords.Add(new RoadNodeFeatureCompareRecord(FeatureType.Extract, extractFeature.RecordNumber, extractFeature.Attributes, extractFeature.Attributes.Id, RecordType.Removed));
            }
        }
    }
}
