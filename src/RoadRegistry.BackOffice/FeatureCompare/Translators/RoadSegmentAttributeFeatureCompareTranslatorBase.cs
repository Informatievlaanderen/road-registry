namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

internal abstract class RoadSegmentAttributeFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadSegmentAttributeFeatureCompareAttributes, new()
{
    private readonly string _fileName;

    protected RoadSegmentAttributeFeatureCompareTranslatorBase(Encoding encoding, string fileName)
        : base(encoding)
    {
        _fileName = fileName;
    }

    protected abstract bool AttributesEquals(Feature<TAttributes> feature1, Feature<TAttributes> feature2);

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, _fileName);

        var wegsegmentenAdd = context.RoadSegments.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegments.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegments.Where(x => x.RecordType == RecordType.Modified).ToList();
        var wegsegmentenDelete = context.RoadSegments.Where(x => x.RecordType == RecordType.Removed).ToList();

        var processedRecords = new List<Record>();

        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var addedFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
            if (addedFeatures.Any())
            {
                foreach (var feature in addedFeatures)
                {
                    feature.Attributes.RoadSegmentId = wegsegment.GetNewOrOriginalId();
                    processedRecords.Add(new Record(feature, RecordType.Added));
                }
            }
        }

        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deletedFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
            if (deletedFeatures.Any())
            {
                foreach (var feature in deletedFeatures)
                {
                    processedRecords.Add(new Record(feature, RecordType.Removed));
                }
            }
        }

        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
            var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);

            if (wegsegmentExtractFeatures.Count != wegsegmentLeveringFeatures.Count)
            {
                foreach (var feature in wegsegmentExtractFeatures)
                {
                    processedRecords.Add(new Record(feature, RecordType.Removed));
                }

                foreach (var feature in wegsegmentLeveringFeatures)
                {
                    processedRecords.Add(new Record(feature, RecordType.Added));
                }
            }
            else
            {
                wegsegmentExtractFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));
                wegsegmentLeveringFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));

                for (var i = 0; i <= wegsegmentLeveringFeatures.Count - 1; i++)
                {
                    var leveringFeature = wegsegmentLeveringFeatures[i];
                    var extractFeature = wegsegmentExtractFeatures[i];

                    if (AttributesEquals(leveringFeature, extractFeature))
                    {
                        processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                    }
                    else
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));

                        processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                    }
                }
            }
        }

        foreach (var wegsegment in wegsegmentenUpdate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);
            var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);

            var removeAndAddLanes = wegsegment.GeometryChanged || wegsegmentExtractFeatures.Count != wegsegmentLeveringFeatures.Count;
            if (removeAndAddLanes)
            {
                foreach (var feature in wegsegmentExtractFeatures)
                {
                    if (processedRecords.All(record => record.Feature.Attributes.Id != feature.Attributes.Id))
                    {
                        processedRecords.Add(new Record(feature, RecordType.Removed));
                    }
                }

                foreach (var feature in wegsegmentLeveringFeatures)
                {
                    processedRecords.Add(new Record(feature, RecordType.Added, wegsegment.Id));
                }
            }
            else
            {
                wegsegmentExtractFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));
                wegsegmentLeveringFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));

                for (var i = 0; i <= wegsegmentLeveringFeatures.Count - 1; i++)
                {
                    var leveringFeature = wegsegmentLeveringFeatures[i];
                    var extractFeature = wegsegmentExtractFeatures[i];

                    if (AttributesEquals(leveringFeature, extractFeature))
                    {
                        processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                    }
                    else
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));

                        processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
                    }
                }
            }
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
