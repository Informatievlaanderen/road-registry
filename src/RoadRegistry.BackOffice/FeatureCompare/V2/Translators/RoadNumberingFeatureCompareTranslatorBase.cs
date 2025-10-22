namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Models;
using RoadSegment.ValueObjects;
using Uploads;

public abstract class RoadNumberingFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadNumberingFeatureCompareAttributes, new()
{
    private readonly ExtractFileName _fileName;

    protected RoadNumberingFeatureCompareTranslatorBase(IZipArchiveFeatureReader<Feature<TAttributes>> featureReader, ExtractFileName fileName)
        : base(featureReader)
    {
        _fileName = fileName;
    }

    protected abstract void HandleIdenticalRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeatures, ILookup<RoadSegmentId, Feature<TAttributes>> extractFeatures, List<Record> processedRecords);
    protected abstract void HandleModifiedRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeatures, ILookup<RoadSegmentId, Feature<TAttributes>> extractFeatures, List<Record> processedRecords);

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, context);

        problems.ThrowIfError();

        problems = ValidateRoadSegmentIds(changeFeatures, problems, context, cancellationToken);

        var extractFeaturesLookup = extractFeatures.ToLookup(x => x.Attributes.RoadSegmentId);
        var changeFeaturesLookup = changeFeatures.ToLookup(x => x.Attributes.RoadSegmentId);

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

        return Task.FromResult((TranslateProcessedRecords(context, changes, processedRecords), problems));
    }

    private static List<Record> ProcessAddedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenAdd = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Added).ToList();
        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentChangeFeatures = changeFeaturesLookup[wegsegment.GetOriginalId()].ToList();
            foreach (var feature in wegsegmentChangeFeatures)
            {
                processedRecords.Add(new Record(feature with
                {
                    Attributes = feature.Attributes with
                    {
                        RoadSegmentId = wegsegment.GetActualId()
                    }
                }, RecordType.Added));
            }
        }

        return processedRecords;
    }

    private static List<Record> ProcessRemovedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenDelete = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Removed).ToList();
        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            processedRecords.AddRange(extractFeaturesLookup[wegsegment.GetActualId()]
                .Select(feature => new Record(feature, RecordType.Removed)));
        }

        return processedRecords;
    }

    private List<Record> ProcessIdenticalSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenIdentical = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Identical).ToList();
        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleIdenticalRoadSegment(wegsegment, changeFeaturesLookup, extractFeaturesLookup, processedRecords);
        }

        return processedRecords;
    }

    private List<Record> ProcessModifiedSegments(ILookup<RoadSegmentId, Feature<TAttributes>> extractFeaturesLookup, ILookup<RoadSegmentId, Feature<TAttributes>> changeFeaturesLookup, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        var processedRecords = new List<Record>();

        var wegsegmentenUpdate = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Modified).ToList();
        foreach (var wegsegment in wegsegmentenUpdate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleModifiedRoadSegment(wegsegment, changeFeaturesLookup, extractFeaturesLookup, processedRecords);
        }

        return processedRecords;
    }

    private ZipArchiveProblems ValidateRoadSegmentIds(List<Feature<TAttributes>> changeFeatures, ZipArchiveProblems problems, ZipArchiveEntryFeatureCompareTranslateContext context, CancellationToken cancellationToken)
    {
        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentFeature = context.FindNotRemovedRoadSegmentByOriginalId(changeFeature.Attributes.RoadSegmentId);
            if (wegsegmentFeature is null)
            {
                var recordContext = _fileName
                    .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentNationalRoadAttributeDbaseRecord.WS_OIDN), changeFeature.Attributes.Id);

                problems += recordContext.RoadSegmentIdOutOfRange(changeFeature.Attributes.RoadSegmentId);
            }
        }

        return problems;
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType);
}
