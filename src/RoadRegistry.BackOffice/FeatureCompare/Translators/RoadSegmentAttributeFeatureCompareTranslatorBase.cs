namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Uploads;

internal abstract class RoadSegmentAttributeFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadSegmentAttributeFeatureCompareAttributes, new()
{
    private readonly ExtractFileName _fileName;

    protected RoadSegmentAttributeFeatureCompareTranslatorBase(Encoding encoding, ExtractFileName fileName)
        : base(encoding)
    {
        _fileName = fileName;
    }

    protected abstract bool AttributesEquals(Feature<TAttributes> feature1, Feature<TAttributes> feature2);

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, _fileName, context);

        var wegsegmentenAdd = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Modified).ToList();
        var wegsegmentenDelete = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Removed).ToList();

        var processedRecords = new List<Record>();

        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var addedFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
            if (addedFeatures.Any())
            {
                foreach (var feature in addedFeatures)
                {
                    feature.Attributes.RoadSegmentId = wegsegment.GetActualId();
                    processedRecords.Add(new Record(feature, RecordType.Added));
                }
            }
        }

        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deletedFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
            if (deletedFeatures.Any())
            {
                processedRecords.AddRange(deletedFeatures.Select(feature => new Record(feature, RecordType.Removed)));
            }
        }

        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
            var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());

            if (wegsegmentExtractFeatures.Count != wegsegmentChangeFeatures.Count)
            {
                processedRecords.AddRange(wegsegmentExtractFeatures.Select(feature => new Record(feature, RecordType.Removed)));

                processedRecords.AddRange(wegsegmentChangeFeatures.Select(feature => new Record(feature, RecordType.Added)));
            }
            else
            {
                wegsegmentExtractFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));
                wegsegmentChangeFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));

                for (var i = 0; i <= wegsegmentChangeFeatures.Count - 1; i++)
                {
                    var changeFeature = wegsegmentChangeFeatures[i];
                    var extractFeature = wegsegmentExtractFeatures[i];

                    if (AttributesEquals(changeFeature, extractFeature))
                    {
                        processedRecords.Add(new Record(changeFeature, RecordType.Identical));
                    }
                    else
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));

                        processedRecords.Add(new Record(changeFeature, RecordType.Added));
                    }
                }
            }
        }

        foreach (var wegsegment in wegsegmentenUpdate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
            var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());

            var removeAndAddLanes = wegsegment.GeometryChanged || wegsegmentExtractFeatures.Count != wegsegmentChangeFeatures.Count;
            if (removeAndAddLanes)
            {
                var removeExtractFeatures = wegsegmentExtractFeatures
                    .Where(feature => processedRecords.All(record => record.Feature.Attributes.Id != feature.Attributes.Id))
                    .ToArray();
                processedRecords.AddRange(removeExtractFeatures.Select(feature => new Record(feature, RecordType.Removed)));
                
                processedRecords.AddRange(wegsegmentChangeFeatures.Select(feature =>
                {
                    feature.Attributes.RoadSegmentId = wegsegment.GetActualId();
                    return new Record(feature, RecordType.Added);
                }));
            }
            else
            {
                wegsegmentExtractFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));
                wegsegmentChangeFeatures.Sort((x, y) => x.Attributes.FromPosition.CompareTo(y.Attributes.FromPosition));

                for (var i = 0; i <= wegsegmentChangeFeatures.Count - 1; i++)
                {
                    var changeFeature = wegsegmentChangeFeatures[i];
                    var extractFeature = wegsegmentExtractFeatures[i];

                    if (AttributesEquals(changeFeature, extractFeature))
                    {
                        processedRecords.Add(new Record(changeFeature, RecordType.Identical));
                    }
                    else
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));

                        changeFeature.Attributes.RoadSegmentId = wegsegment.GetActualId();
                        processedRecords.Add(new Record(changeFeature, RecordType.Added));
                    }
                }
            }
        }

        return Task.FromResult((TranslateProcessedRecords(changes, processedRecords), problems));
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType);
}
