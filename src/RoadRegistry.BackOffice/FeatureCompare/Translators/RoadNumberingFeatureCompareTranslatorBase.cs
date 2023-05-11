namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

internal abstract class RoadNumberingFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadNumberingFeatureCompareAttributes, new()
{
    private readonly string _fileName;

    protected RoadNumberingFeatureCompareTranslatorBase(Encoding encoding, string fileName)
        : base(encoding)
    {
        _fileName = fileName;
    }

    protected abstract void HandleIdenticalRoadSegment(RoadSegmentRecord wegsegment, List<Feature<TAttributes>> changeFeatures, List<Feature<TAttributes>> extractFeatures, List<Record> processedRecords);
    protected abstract void HandleModifiedRoadSegment(RoadSegmentRecord wegsegment, List<Feature<TAttributes>> changeFeatures, List<Feature<TAttributes>> extractFeatures, List<Record> processedRecords);

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, changeFeatures) = ReadExtractAndChangeFeatures(entries, _fileName);

        var wegsegmentenAdd = context.RoadSegments.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegments.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegments.Where(x => x.RecordType == RecordType.Modified).ToList();
        var wegsegmentenDelete = context.RoadSegments.Where(x => x.RecordType == RecordType.Removed).ToList();

        var processedRecords = new List<Record>();

        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
            foreach (var feature in wegsegmentChangeFeatures)
            {
                processedRecords.Add(new Record(feature, RecordType.Added, wegsegment.GetNewOrOriginalId()));
            }
        }

        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
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

        foreach (var record in processedRecords.Where(x => x.TempRoadSegmentId != 0))
        {
            record.Feature.Attributes.RoadSegmentId = record.TempRoadSegmentId;
        }

        return Task.FromResult(TranslateProcessedRecords(changes, processedRecords));
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType, int TempRoadSegmentId = 0);
}
