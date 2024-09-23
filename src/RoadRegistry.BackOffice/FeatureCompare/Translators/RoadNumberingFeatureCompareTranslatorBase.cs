namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Extracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    protected abstract void HandleIdenticalRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<TAttributes>> changeFeatures, List<Feature<TAttributes>> extractFeatures, List<Record> processedRecords);
    protected abstract void HandleModifiedRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<TAttributes>> changeFeatures, List<Feature<TAttributes>> extractFeatures, List<Record> processedRecords);

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, _fileName, context);

        problems.ThrowIfError();

        //TODO-rik check if changeFeatures verwijzen naar bestaande wegsegment, zie gradeseparatedjunction FC translator
        var wegsegmentenAdd = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Modified).ToList();
        var wegsegmentenDelete = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Removed).ToList();

        var processedRecords = new List<Record>();

        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
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

        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetActualId());
            foreach (var feature in wegsegmentExtractFeatures)
            {
                processedRecords.Add(new Record(feature, RecordType.Removed));
            }
        }

        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleIdenticalRoadSegment(wegsegment, changeFeatures, extractFeatures, processedRecords);
        }

        foreach (var wegsegment in wegsegmentenUpdate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandleModifiedRoadSegment(wegsegment, changeFeatures, extractFeatures, processedRecords);
        }

        problems.ThrowIfError();

        return Task.FromResult((TranslateProcessedRecords(context, changes, processedRecords), problems));
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType);
}
