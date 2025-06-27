namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Models;
using Uploads;

public abstract class RoadSegmentAttributeFeatureCompareTranslatorBase<TAttributes> : FeatureCompareTranslatorBase<TAttributes>
    where TAttributes : RoadSegmentAttributeFeatureCompareAttributes, new()
{
    protected RoadSegmentAttributeFeatureCompareTranslatorBase(IZipArchiveFeatureReader<Feature<TAttributes>> featureReader)
        : base(featureReader)
    {
    }

    protected abstract bool AttributesEquals(Feature<TAttributes> feature1, Feature<TAttributes> feature2);

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, context);

        problems.ThrowIfError();

        var wegsegmentenAdd = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Modified).ToList();
        var wegsegmentenDelete = context.RoadSegmentRecords.Where(x => x.RecordType == RecordType.Removed).ToList();

        var extractFeaturesLookup = extractFeatures.ToLookup(x => x.Attributes.RoadSegmentId);
        var changeFeaturesLookup = changeFeatures.ToLookup(x => x.Attributes.RoadSegmentId);

        var processedRecords = new List<Record>();

        foreach (var wegsegment in wegsegmentenAdd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var addedFeatures = changeFeaturesLookup[wegsegment.GetOriginalId()].ToList();
            if (addedFeatures.Any())
            {
                foreach (var feature in addedFeatures)
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
        }

        foreach (var wegsegment in wegsegmentenDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();

            processedRecords.AddRange(extractFeaturesLookup[wegsegment.GetOriginalId()]
                .Select(feature => new Record(feature, RecordType.Removed)));
        }

        foreach (var wegsegment in wegsegmentenIdentical)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wegsegmentExtractFeatures = extractFeaturesLookup[wegsegment.GetOriginalId()].ToList();
            var wegsegmentChangeFeatures = changeFeaturesLookup[wegsegment.GetOriginalId()].ToList();

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

            var wegsegmentExtractFeatures = extractFeaturesLookup[wegsegment.GetOriginalId()].ToList();
            var wegsegmentChangeFeatures = changeFeaturesLookup[wegsegment.GetOriginalId()].ToList();
            var wegsegmentIdChanged = wegsegment.GetOriginalId() != wegsegment.GetActualId();

            var removeAndAddLanes = wegsegment.GeometryChanged
                                    || wegsegmentExtractFeatures.Count != wegsegmentChangeFeatures.Count
									|| wegsegmentIdChanged;
            if (removeAndAddLanes)
            {
                var removeExtractFeatures = wegsegmentExtractFeatures
                    .Where(feature => processedRecords.All(record => record.Feature.Attributes.Id != feature.Attributes.Id))
                    .ToArray();
                processedRecords.AddRange(removeExtractFeatures.Select(feature => new Record(feature, RecordType.Removed)));

                processedRecords.AddRange(wegsegmentChangeFeatures.Select(feature => new Record(feature with
                {
                    Attributes = feature.Attributes with
                    {
                        RoadSegmentId = wegsegment.GetActualId()
                    }
                }, RecordType.Added)));
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

                        processedRecords.Add(new Record(changeFeature with
                        {
                            Attributes = changeFeature.Attributes with
                            {
                                RoadSegmentId = wegsegment.GetActualId()
                            }
                        }, RecordType.Added));
                    }
                }
            }
        }

        problems.ThrowIfError();

        return Task.FromResult((TranslateProcessedRecords(changes, processedRecords), problems));
    }

    protected abstract TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records);

    protected record Record(Feature<TAttributes> Feature, RecordType RecordType);
}
