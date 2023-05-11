namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;
using Uploads;

internal class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, "RLTOGKRUISING");

        var processedRecords = new List<Record>();

        void RemoveFeatures(ICollection<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> features)
        {
            foreach (var feature in features)
            {
                if (!processedRecords.Any(x => x.Feature.Attributes.Id == feature.Attributes.Id
                                               && x.RecordType.Equals(RecordType.Removed)))
                {
                    processedRecords.Add(new Record(feature, RecordType.Removed));
                }
            }
        }

        foreach (var leveringFeature in leveringFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var boWegsegmentFeature = context.RoadSegments.SingleOrDefault(x => !x.RecordType.Equals(RecordType.Removed) && x.Id == leveringFeature.Attributes.UpperRoadSegmentId);
            if (boWegsegmentFeature is null)
            {
                throw new RoadSegmentNotFoundInZipArchiveException(leveringFeature.Attributes.UpperRoadSegmentId);
            }

            var onWegsegmentFeature = context.RoadSegments.SingleOrDefault(x => !x.RecordType.Equals(RecordType.Removed) && x.Id == leveringFeature.Attributes.LowerRoadSegmentId);
            if (onWegsegmentFeature is null)
            {
                throw new RoadSegmentNotFoundInZipArchiveException(leveringFeature.Attributes.LowerRoadSegmentId);
            }

            var editedLeveringFeature = leveringFeature with
            {
                Attributes = leveringFeature.Attributes with
                {
                    UpperRoadSegmentId = boWegsegmentFeature.GetNewOrOriginalId(),
                    LowerRoadSegmentId = onWegsegmentFeature.GetNewOrOriginalId()
                }
            };

            var matchingExtractFeatures = extractFeatures
                .Where(x => x.Attributes.UpperRoadSegmentId == editedLeveringFeature.Attributes.UpperRoadSegmentId
                            && x.Attributes.LowerRoadSegmentId == editedLeveringFeature.Attributes.LowerRoadSegmentId)
                .ToArray();
            if (!matchingExtractFeatures.Any())
            {
                processedRecords.Add(new Record(editedLeveringFeature, RecordType.Added));
                continue;
            }

            var hasMatchByType = matchingExtractFeatures.Any(x => x.Attributes.Type == editedLeveringFeature.Attributes.Type);
            if (hasMatchByType)
            {
                processedRecords.Add(new Record(editedLeveringFeature, RecordType.Identical));
                continue;
            }

            var matchingExtractFeature = matchingExtractFeatures.FirstOrDefault(x => x.Attributes.Id == editedLeveringFeature.Attributes.Id)
                                         ?? matchingExtractFeatures.First();

            processedRecords.Add(new Record(editedLeveringFeature, RecordType.Added));
            processedRecords.Add(new Record(matchingExtractFeature, RecordType.Removed));
        }

        {
            var extractFeaturesWithoutLeveringFeatures = extractFeatures.FindAll(extractFeature =>
                !processedRecords.Any(x => x.Feature.Attributes.UpperRoadSegmentId == extractFeature.Attributes.UpperRoadSegmentId
                                           && x.Feature.Attributes.LowerRoadSegmentId == extractFeature.Attributes.LowerRoadSegmentId)
            );

            RemoveFeatures(extractFeaturesWithoutLeveringFeatures);
        }

        foreach (var record in processedRecords)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            new GradeSeparatedJunctionId(record.Feature.Attributes.Id),
                            GradeSeparatedJunctionType.ByIdentifier[record.Feature.Attributes.Type],
                            new RoadSegmentId(record.Feature.Attributes.UpperRoadSegmentId),
                            new RoadSegmentId(record.Feature.Attributes.LowerRoadSegmentId)
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            new GradeSeparatedJunctionId(record.Feature.Attributes.Id)
                        )
                    );
                    break;
            }
        }

        return Task.FromResult(changes);
    }

    private record Record(Feature<GradeSeparatedJunctionFeatureCompareAttributes> Feature, RecordType RecordType);
}
