namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

public abstract class RoadNumberingFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadNumberingFeatureCompareAttributes, new()
{
    private readonly ExtractFileName _fileName;

    protected RoadNumberingFeatureCompareTranslatorBase(IZipArchiveFeatureReader<Feature<TAttributes>> featureReader, ExtractFileName fileName)
        : base(featureReader)
    {
        _fileName = fileName;
    }

    protected abstract void HandleIdenticalRoadSegment(RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentId, Feature<TAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<TAttributes>> extractFeatures,
        List<Record> processedRecords,
        ZipArchiveEntryFeatureCompareTranslateContext context);
    protected abstract void HandleModifiedRoadSegment(RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentId, Feature<TAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<TAttributes>> extractFeatures,
        List<Record> processedRecords,
        ZipArchiveEntryFeatureCompareTranslateContext context);

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var features = ReadExtractAndChangeFeatures(context.Archive, context);
        var problems = features.Problems;
        problems.ThrowIfError();

        problems += ValidateRoadSegmentTempIds(features.Change, problems, context, cancellationToken);
        problems.ThrowIfError();

        var extractFeaturesLookup = features.Extract.ToLookup(x => context.MapToRoadSegmentId(FeatureType.Extract, x.Attributes.RoadSegmentTempId));
        var changeFeaturesLookup = features.Change.ToLookup(x => context.MapToRoadSegmentId(FeatureType.Change, x.Attributes.RoadSegmentTempId));

        List<Record> addedRecords = [], deletedRecords = [], identicalRecords = [], modifiedRecords = [];

        Parallel.Invoke([
            () => addedRecords = ProcessAddedSegments(changeFeaturesLookup, context, cancellationToken),
            () => deletedRecords = ProcessRemovedSegments(extractFeaturesLookup, context, cancellationToken),
            () => identicalRecords = ProcessIdenticalSegments(extractFeaturesLookup, changeFeaturesLookup, context, cancellationToken),
            () => modifiedRecords = ProcessModifiedSegments(extractFeaturesLookup, changeFeaturesLookup, context, cancellationToken)
        ]);

        var processedRecords = Enumerable.Empty<Record>()
            .Concat(addedRecords)
            .Concat(deletedRecords)
            .Concat(identicalRecords)
            .Concat(modifiedRecords)
            .ToList();

        return Task.FromResult((TranslateProcessedRecords(changes, processedRecords), problems));
    }

    private static List<Record> ProcessAddedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenAdd = context.GetRoadSegmentRecords(FeatureType.Change)
            .Where(x => x.RecordType == RecordType.Added)
            .ToList();
        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentChangeFeatures = changeFeaturesLookup[wegsegment.GetActualId()].ToList();
            foreach (var feature in wegsegmentChangeFeatures)
            {
                processedRecords.Add(new Record(feature, RecordType.Added, wegsegment.GetActualId()));
            }
        }

        return processedRecords;
    }

    private static List<Record> ProcessRemovedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenDelete = context.GetRoadSegmentRecords(FeatureType.Change)
            .Where(x => x.RecordType == RecordType.Removed)
            .ToList();
        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roadSegmentId = wegsegment.GetActualId();
            processedRecords.AddRange(extractFeaturesLookup[roadSegmentId]
                .Select(feature => new Record(feature, RecordType.Removed, roadSegmentId)));
        }

        return processedRecords;
    }

    private List<Record> ProcessIdenticalSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenIdentical = context.GetRoadSegmentRecords(FeatureType.Change)
            .Where(x => x.RecordType == RecordType.Identical).ToList();
        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleIdenticalRoadSegment(wegsegment, changeFeaturesLookup, extractFeaturesLookup, processedRecords, context);
        }

        return processedRecords;
    }

    private List<Record> ProcessModifiedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenUpdate = context.GetRoadSegmentRecords(FeatureType.Change)
            .Where(x => x.RecordType == RecordType.Modified)
            .ToList();
        foreach (var wegsegment in wegsegmentenUpdate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleModifiedRoadSegment(wegsegment, changeFeaturesLookup, extractFeaturesLookup, processedRecords, context);
        }

        return processedRecords;
    }

    private ZipArchiveProblems ValidateRoadSegmentTempIds(List<Feature<TAttributes>> changeFeatures, ZipArchiveProblems problems, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentFeature = context.FindNotRemovedRoadSegmentByTempId(FeatureType.Change, changeFeature.Attributes.RoadSegmentTempId);
            if (wegsegmentFeature is null)
            {
                var recordContext = _fileName
                    .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentNationalRoadAttributeDbaseRecord.WS_TEMPID), changeFeature.Attributes.RoadSegmentTempId.ToInt());

                problems += recordContext.RoadSegmentIdOutOfRange(changeFeature.Attributes.RoadSegmentTempId.ToInt());
            }
        }

        return problems;
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType, RoadSegmentId RoadSegmentId);
}
