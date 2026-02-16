namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.Infrastructure;
using RoadSegment;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    private const ExtractFileName FileName = ExtractFileName.RltOgkruising;

    public GradeSeparatedJunctionFeatureCompareTranslator(
        GradeSeparatedJunctionFeatureCompareFeatureReader featureReader)
        : base(featureReader)
    {
    }

    public override async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var features = ReadExtractAndChangeFeatures(context.Archive, context);
        var problems = features.Problems;
        problems.ThrowIfError();

        var processedRecords = new List<Record>();

        List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> GetFeaturesToRemove()
        {
            var featuresToRemove = new List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>();
            var usedProcessedRecords = new List<Record>();

            foreach (var extractFeature in features.Extract)
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

        foreach (var changeFeature in features.Change)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordContext = FileName
                .AtDbaseRecord(FeatureType.Change, changeFeature.RecordNumber)
                .WithIdentifier(nameof(GradeSeparatedJunctionDbaseRecord.OK_OIDN), changeFeature.Attributes.Id);

            if (changeFeature.Attributes.LowerRoadSegmentId == changeFeature.Attributes.UpperRoadSegmentId)
            {
                problems += recordContext.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment(changeFeature.Attributes.LowerRoadSegmentId);
                continue;
            }

            var boWegsegmentFeature = context.FindNotRemovedRoadSegmentByOriginalId(changeFeature.Attributes.UpperRoadSegmentId);
            var onWegsegmentFeature = context.FindNotRemovedRoadSegmentByOriginalId(changeFeature.Attributes.LowerRoadSegmentId);

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

            var matchingExtractFeatures = features.Extract
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
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = record.Feature.Attributes.Id,
                            LowerRoadSegmentId = record.Feature.Attributes.LowerRoadSegmentId,
                            UpperRoadSegmentId = record.Feature.Attributes.UpperRoadSegmentId,
                            Type = record.Feature.Attributes.Type
                        }
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = record.Feature.Attributes.Id
                        }
                    );
                    break;
            }
        }

        return changes;
    }

    private Task<ZipArchiveProblems> ValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction(ZipArchiveEntryFeatureCompareTranslateContext context, List<Record> processedRecords)
    {
        var problems = ZipArchiveProblems.None;

        var changedRoadSegments = context.RoadSegmentRecords
            .NotOutlined()
            .Where(x => x.FeatureType == FeatureType.Change
                        && (x.RecordType == RecordType.Added || (x.RecordType == RecordType.Modified && x.GeometryChanged)))
            .ToList();

        var batchCount = Debugger.IsAttached ? 1 : 4;

        var allProblemsForMissingGradeSeparatedJunctions = new ConcurrentDictionary<int, ZipArchiveProblems>();
        Parallel.Invoke(changedRoadSegments
            .SplitIntoBatches(batchCount)
            .Select((changedRoadSegmentsBatch, index) => { return (Action)(() => { allProblemsForMissingGradeSeparatedJunctions.TryAdd(index, GetProblemsForMissingGradeSeparatedJunctions(context, processedRecords, changedRoadSegmentsBatch)); }); })
            .ToArray());

        foreach (var problemsForMissingGradeSeparatedJunctions in allProblemsForMissingGradeSeparatedJunctions.OrderBy(x => x.Key).Select(x => x.Value))
        {
            problems += problemsForMissingGradeSeparatedJunctions;
        }

        return Task.FromResult(problems);
    }

    private ZipArchiveProblems GetProblemsForMissingGradeSeparatedJunctions(
        ZipArchiveEntryFeatureCompareTranslateContext context,
        List<Record> processedRecords,
        ICollection<RoadSegmentFeatureCompareRecord> changedRoadSegments)
    {
        var uniqueRoadSegmentCombinations = (
            from r1 in changedRoadSegments
            from r2 in context.RoadSegmentRecords.NotRemoved().NotOutlined()
            where r1.Id != r2.Id && r1.Attributes.Geometry.Envelope.Intersects(r2.Attributes.Geometry.Envelope)
            select new RoadSegmentCombination(r1, r2)
        ).DistinctBy(x => x.Key).ToList();

        var gradeSeparatedJunctions = processedRecords
            .Where(x => x.RecordType != RecordType.Removed)
            .Select(x => new
            {
                x.Feature.Attributes,
                CombinationKey = new RoadSegmentCombinationKey(x.Feature.Attributes.LowerRoadSegmentId, x.Feature.Attributes.UpperRoadSegmentId)
            })
            .ToList();

        var roadSegmentIntersections = (
                from combination in uniqueRoadSegmentCombinations
                let intersectionGeometry = combination.RoadSegment1.Attributes.Geometry.Intersection(combination.RoadSegment2.Attributes.Geometry)
                where intersectionGeometry is Point || intersectionGeometry is MultiPoint
                let intersections = intersectionGeometry.ToMultiPoint()
                let gradeSeparatedJunctionsCount = gradeSeparatedJunctions.Count(grade => grade.CombinationKey.Equals(combination.Key))
                let r1Geometry = combination.RoadSegment1.Attributes.Geometry.GetSingleLineString()
                let r2Geometry = combination.RoadSegment2.Attributes.Geometry.GetSingleLineString()
                from intersection in intersections.OfType<Point>()
                let intersectionIsFarAwayFromStartEndPoints = intersection.IsFarEnoughAwayFrom([r1Geometry.StartPoint, r1Geometry.EndPoint, r2Geometry.StartPoint, r2Geometry.EndPoint], context.Tolerances.GeometryTolerance)
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
        private RoadSegmentCombinationKey? _key;
        public RoadSegmentCombinationKey Key => _key ??= new RoadSegmentCombinationKey(RoadSegment1, RoadSegment2);
    }

    private sealed class RoadSegmentCombinationKey :
        IEquatable<RoadSegmentCombinationKey>,
        IComparable
    {
        private readonly int _min;
        private readonly int _max;

        public RoadSegmentCombinationKey(RoadSegmentFeatureCompareRecord roadSegment1, RoadSegmentFeatureCompareRecord roadSegment2)
            : this(roadSegment1.Id, roadSegment2.Id)
        {
        }

        public RoadSegmentCombinationKey(RoadSegmentId id1, RoadSegmentId id2)
        {
            _min = Math.Min(id1, id2);
            _max = Math.Max(id1, id2);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is RoadSegmentCombinationKey other && Equals(other);
        }

        public bool Equals(RoadSegmentCombinationKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _min == other._min && _max == other._max;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_min, _max);
        }

        public int CompareTo(object obj)
        {
            if (obj is RoadSegmentCombinationKey other)
            {
                return (_min, _max).CompareTo((other._min, other._max));
            }

            return -1;
        }
    }

    private sealed record Record(Feature<GradeSeparatedJunctionFeatureCompareAttributes> Feature, RecordType RecordType);
}
