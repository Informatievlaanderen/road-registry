namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using NetTopologySuite.Geometries;
using Uploads;

internal class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.RltOgkruising;

    public GradeSeparatedJunctionFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
    {
    }

    protected override (List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var featureReader = new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(archive, featureType, fileName, context);
    }

    public override Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, FileName, context);

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

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName.AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber);

            if (changeFeature.Attributes.LowerRoadSegmentId == changeFeature.Attributes.UpperRoadSegmentId)
            {
                problems += recordContext.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment(changeFeature.Attributes.LowerRoadSegmentId);
                continue;
            }

            var boWegsegmentFeature = context.GetNonRemovedRoadSegmentRecords().SingleOrDefault(x => x.GetOriginalId() == changeFeature.Attributes.UpperRoadSegmentId);
            var onWegsegmentFeature = context.GetNonRemovedRoadSegmentRecords().SingleOrDefault(x => x.GetOriginalId() == changeFeature.Attributes.LowerRoadSegmentId);
            
            if (boWegsegmentFeature is null || onWegsegmentFeature is null)
            {
                if (boWegsegmentFeature is null)
                {
                    problems += recordContext.UpperRoadSegmentIdOutOfRange(changeFeature.Attributes.UpperRoadSegmentId);
                }
                if (onWegsegmentFeature is null)
                {
                    problems += recordContext.LowerRoadSegmentIdOutOfRange(changeFeature.Attributes.LowerRoadSegmentId);
                }
                continue;
            }

            var editedChangeFeature = changeFeature with
            {
                Attributes = changeFeature.Attributes with
                {
                    UpperRoadSegmentId = boWegsegmentFeature.GetActualId(),
                    LowerRoadSegmentId = onWegsegmentFeature.GetActualId()
                }
            };

            var matchingExtractFeatures = extractFeatures
                .Where(x => x.Attributes.UpperRoadSegmentId == editedChangeFeature.Attributes.UpperRoadSegmentId
                            && x.Attributes.LowerRoadSegmentId == editedChangeFeature.Attributes.LowerRoadSegmentId)
                .ToArray();
            if (!matchingExtractFeatures.Any())
            {
                processedRecords.Add(new Record(editedChangeFeature, RecordType.Added));
                continue;
            }

            var hasMatchByType = matchingExtractFeatures.Any(x => x.Attributes.Type == editedChangeFeature.Attributes.Type);
            if (hasMatchByType)
            {
                processedRecords.Add(new Record(editedChangeFeature, RecordType.Identical));
                continue;
            }

            var matchingExtractFeature = matchingExtractFeatures.FirstOrDefault(x => x.Attributes.Id == editedChangeFeature.Attributes.Id)
                                         ?? matchingExtractFeatures.First();

            processedRecords.Add(new Record(editedChangeFeature, RecordType.Added));
            processedRecords.Add(new Record(matchingExtractFeature, RecordType.Removed));
        }

        {
            var extractFeaturesWithoutChangeFeatures = extractFeatures.FindAll(extractFeature =>
                !processedRecords.Any(x => x.Feature.Attributes.UpperRoadSegmentId == extractFeature.Attributes.UpperRoadSegmentId
                                           && x.Feature.Attributes.LowerRoadSegmentId == extractFeature.Attributes.LowerRoadSegmentId)
            );

            RemoveFeatures(extractFeaturesWithoutChangeFeatures);
        }

        problems += ValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction(context, processedRecords);

        foreach (var record in processedRecords)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
                            record.Feature.Attributes.Type,
                            record.Feature.Attributes.UpperRoadSegmentId,
                            record.Feature.Attributes.LowerRoadSegmentId
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id
                        )
                    );
                    break;
            }
        }

        return Task.FromResult((changes, problems));
    }

    private ZipArchiveProblems ValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction(ZipArchiveEntryFeatureCompareTranslateContext context, List<Record> processedRecords)
    {
        var problems = ZipArchiveProblems.None;

        string CreateCombinationKey(RoadSegmentId id1, RoadSegmentId id2)
        {
            return $"{Math.Min(id1, id2)}_{Math.Max(id1, id2)}";
        }

        var gradeSeparatedJunctions = processedRecords
            .Where(x => x.RecordType != RecordType.Removed)
            .Select(x => new
            {
                x.Feature.Attributes,
                CombinationKey = CreateCombinationKey(x.Feature.Attributes.LowerRoadSegmentId, x.Feature.Attributes.UpperRoadSegmentId)
            })
            .ToList();

        var roadSegmentIntersections = (
            from r1 in context.RoadSegmentRecords.Where(x => x.FeatureType == FeatureType.Change)
            from r2 in context.GetNonRemovedRoadSegmentRecords()
            where r1.Id != r2.Id
            let intersectionGeometry = r1.Attributes.Geometry.Intersection(r2.Attributes.Geometry)
            where intersectionGeometry is Point || intersectionGeometry is MultiPoint
            let intersections = intersectionGeometry.ToMultiPoint()
            let combinationKey = CreateCombinationKey(r1.Id, r2.Id)
            let gradeSeparatedJunctionsCount = gradeSeparatedJunctions.Count(grade => grade.CombinationKey == combinationKey)
            let r1Geometry = r1.Attributes.Geometry.GetSingleLineString()
            let r2Geometry = r2.Attributes.Geometry.GetSingleLineString()
            from intersection in intersections.OfType<Point>()
            let intersectionIsFarAwayFromStartEndPoints = intersection.IsFarEnoughAwayFrom(new[] { r1Geometry.StartPoint, r1Geometry.EndPoint, r2Geometry.StartPoint, r2Geometry.EndPoint}, context.Tolerances.MeasurementTolerance)
            where intersectionIsFarAwayFromStartEndPoints
            select new
            {
                RoadSegment1 = r1,
                RoadSegment2 = r2,
                CombinationKey = combinationKey,
                Intersection = intersection,
                GradeSeparatedJunctionsCount = gradeSeparatedJunctionsCount
            }
        )
            .DistinctBy(x => new { x.CombinationKey, Wkt = x.Intersection.ToText()})
            .OrderBy(x => x.CombinationKey)
            .ThenBy(x => x.Intersection)
            .ToList();

        var roadSegmentIntersectionsWithoutGradeSeparatedJunction = roadSegmentIntersections
            .Where(x => x.GradeSeparatedJunctionsCount == 0)
            .ToList();

        foreach (var i in roadSegmentIntersectionsWithoutGradeSeparatedJunction)
        {
            var recordContext = ExtractFileName.Wegsegment.AtDbaseRecord(FeatureType.Change, i.RoadSegment1.RecordNumber);
            
            problems += recordContext.GradeSeparatedJunctionMissing(i.RoadSegment1.GetOriginalId(), i.RoadSegment2.GetOriginalId(), i.Intersection.X, i.Intersection.Y);
        }

        //TODO-rik #WR-470 once it's refined
        //var roadSegmentIntersectionsWithNotEnoughGradeSeparatedJunctions = roadSegmentIntersections
        //    .Where(x => x.Intersections.Count != x.GradeSeparatedJunctionsCount)
        //    .ToArray();

        //foreach (var i in roadSegmentIntersectionsWithNotEnoughGradeSeparatedJunctions)
        //{
        //    var recordContext = ExtractFileName.Wegsegment.AtDbaseRecord(FeatureType.Change, i.RoadSegment1.RecordNumber);

        //    problems += recordContext.ExpectedGradeSeparatedJunctionsCountDiffersFromActual(i.RoadSegment1.Id, i.RoadSegment2.Id, i.Intersections.Count, i.GradeSeparatedJunctionsCount);
        //}

        return problems;
    }

    private sealed record Record(Feature<GradeSeparatedJunctionFeatureCompareAttributes> Feature, RecordType RecordType);
}
