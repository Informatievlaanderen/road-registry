namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Extracts;
using Extracts.Dbase.GradeSeparatedJuntions;
using FeatureToggles;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    private readonly UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle _useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle;
    private const ExtractFileName FileName = ExtractFileName.RltOgkruising;

    public GradeSeparatedJunctionFeatureCompareTranslator(
        GradeSeparatedJunctionFeatureCompareFeatureReader featureReader,
        UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle)
        : base(featureReader)
    {
        _useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle = useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle;
    }
    
    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var (extractFeatures, changeFeatures, problems) = ReadExtractAndChangeFeatures(context.Archive, FileName, context);

        problems.ThrowIfError();

        var processedRecords = new List<Record>();

        List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> GetFeaturesToRemove()
        {
            var featuresToRemove = new List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>();
            var usedProcessedRecords = new List<Record>();

            foreach (var extractFeature in extractFeatures)
            {
                var matchingProcessedRecords = processedRecords
                    .Where(x => x.Feature.Attributes.UpperRoadSegmentId == extractFeature.Attributes.UpperRoadSegmentId
                                && x.Feature.Attributes.LowerRoadSegmentId == extractFeature.Attributes.LowerRoadSegmentId
                                && !usedProcessedRecords.Contains(x))
                    .ToList();

                if (!matchingProcessedRecords.Any())
                {
                    featuresToRemove.Add(extractFeature);
                }
                else
                {
                    usedProcessedRecords.AddRange(matchingProcessedRecords);
                }
            }

            return featuresToRemove;
        }

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

        RoadSegmentFeatureCompareRecord FindRoadSegmentByOriginalId(RoadSegmentId originalId)
        {
            var matchingFeatures = context.RoadSegmentRecords
                .NotRemoved()
                .Where(x => x.GetOriginalId() == originalId)
                .ToList();

            if (matchingFeatures.Count > 1)
            {
                var matchingFeaturesInfo = string.Join("\n", matchingFeatures.Select(feature => $"RoadSegment #{feature.RecordNumber}, ID: {feature.Id}, FeatureType: {feature.FeatureType}, RecordType: {feature.RecordType}"));
                throw new InvalidOperationException($"Found {matchingFeatures.Count} processed road segments with original ID {originalId} while only 1 is expected.\n{matchingFeaturesInfo}");
            }

            return matchingFeatures.SingleOrDefault();
        }

        foreach (var changeFeature in changeFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                .WithIdentifier(nameof(GradeSeparatedJunctionDbaseRecord.OK_OIDN), changeFeature.Attributes.Id);

            if (_useGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle.FeatureEnabled)
            {
                if (changeFeature.Attributes.LowerRoadSegmentId == changeFeature.Attributes.UpperRoadSegmentId)
                {
                    problems += recordContext.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment(changeFeature.Attributes.LowerRoadSegmentId);
                    continue;
                }
            }

            var boWegsegmentFeature = FindRoadSegmentByOriginalId(changeFeature.Attributes.UpperRoadSegmentId);
            var onWegsegmentFeature = FindRoadSegmentByOriginalId(changeFeature.Attributes.LowerRoadSegmentId);

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

        RemoveFeatures(GetFeaturesToRemove());

        problems += await ValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction(context, processedRecords);

        problems.ThrowIfError();

        changes = TranslateProcessedRecords(changes, processedRecords, cancellationToken);

        return (changes, problems);
    }

    private TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        return changes;
    }

    private async Task<ZipArchiveProblems> ValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction(ZipArchiveEntryFeatureCompareTranslateContext context, List<Record> processedRecords)
    {
        var problems = ZipArchiveProblems.None;

        var changedRoadSegments = context.RoadSegmentRecords
            .NotOutlined()
            .Where(x => x.FeatureType == FeatureType.Change
                        && (x.RecordType == RecordType.Added || (x.RecordType == RecordType.Modified && x.GeometryChanged)))
            .ToList();

        var uniqueRoadSegmentCombinations = (
            from r1 in changedRoadSegments
            from r2 in context.RoadSegmentRecords.NotRemoved().NotOutlined()
            where r1.Id != r2.Id
            select new RoadSegmentCombination(r1, r2)
        ).DistinctBy(x => x.Key).ToList();

        var batchCount = Debugger.IsAttached ? 1 : 2;

        var allProblemsForMissingGradeSeparatedJunctions = await Task.WhenAll(
            uniqueRoadSegmentCombinations.SplitIntoBatches(batchCount)
                .Select(uniqueRoadSegmentCombinationsBatch => Task.Run(() =>
                        GetProblemsForMissingGradeSeparatedJunctions(context, processedRecords, uniqueRoadSegmentCombinationsBatch))
                ));
        foreach (var problemsForMissingGradeSeparatedJunctions in allProblemsForMissingGradeSeparatedJunctions)
        {
            problems += problemsForMissingGradeSeparatedJunctions;
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

    private ZipArchiveProblems GetProblemsForMissingGradeSeparatedJunctions(ZipArchiveEntryFeatureCompareTranslateContext context, List<Record> processedRecords, ICollection<RoadSegmentCombination> uniqueRoadSegmentCombinations)
    {
        var gradeSeparatedJunctions = processedRecords
            .Where(x => x.RecordType != RecordType.Removed)
            .Select(x => new
            {
                x.Feature.Attributes,
                CombinationKey = RoadSegmentCombination.CreateKey(x.Feature.Attributes.LowerRoadSegmentId, x.Feature.Attributes.UpperRoadSegmentId)
            })
            .ToList();

        var roadSegmentIntersections = (
                from combination in uniqueRoadSegmentCombinations
                let intersectionGeometry = combination.RoadSegment1.Attributes.Geometry.Intersection(combination.RoadSegment2.Attributes.Geometry)
                where intersectionGeometry is Point || intersectionGeometry is MultiPoint
                let intersections = intersectionGeometry.ToMultiPoint()
                let gradeSeparatedJunctionsCount = gradeSeparatedJunctions.Count(grade => grade.CombinationKey == combination.Key)
                let r1Geometry = combination.RoadSegment1.Attributes.Geometry.GetSingleLineString()
                let r2Geometry = combination.RoadSegment2.Attributes.Geometry.GetSingleLineString()
                from intersection in intersections.OfType<Point>()
                let intersectionIsFarAwayFromStartEndPoints = intersection.IsFarEnoughAwayFrom(new[] { r1Geometry.StartPoint, r1Geometry.EndPoint, r2Geometry.StartPoint, r2Geometry.EndPoint }, context.Tolerances.GeometryTolerance)
                where intersectionIsFarAwayFromStartEndPoints
                select new
                {
                    combination.RoadSegment1,
                    combination.RoadSegment2,
                    CombinationKey = combination.Key,
                    Intersection = intersection,
                    GradeSeparatedJunctionsCount = gradeSeparatedJunctionsCount
                }
            )
            .DistinctBy(x => new { x.CombinationKey, Wkt = x.Intersection.ToText() })
            .OrderBy(x => x.CombinationKey)
            .ThenBy(x => x.Intersection)
            .ToList();

        var roadSegmentIntersectionsWithoutGradeSeparatedJunction = roadSegmentIntersections
            .Where(x => x.GradeSeparatedJunctionsCount == 0)
            .ToList();

        var problems = ZipArchiveProblems.None;

        foreach (var i in roadSegmentIntersectionsWithoutGradeSeparatedJunction)
        {
            var recordContext = ExtractFileName.Wegsegment.AtDbaseRecord(FeatureType.Change, i.RoadSegment1.RecordNumber);

            problems += recordContext.GradeSeparatedJunctionMissing(i.RoadSegment1.GetOriginalId(), i.RoadSegment2.GetOriginalId(), i.Intersection.X, i.Intersection.Y);
        }

        return problems;
    }

    private sealed record RoadSegmentCombination(RoadSegmentFeatureCompareRecord RoadSegment1, RoadSegmentFeatureCompareRecord RoadSegment2)
    {
        private string _key;
        public string Key => _key ??= CreateKey(RoadSegment1.Id, RoadSegment2.Id);

        public static string CreateKey(RoadSegmentId id1, RoadSegmentId id2)
        {
            return $"{Math.Min(id1, id2)}_{Math.Max(id1, id2)}";
        }
    }
    private sealed record Record(Feature<GradeSeparatedJunctionFeatureCompareAttributes> Feature, RecordType RecordType);
}
