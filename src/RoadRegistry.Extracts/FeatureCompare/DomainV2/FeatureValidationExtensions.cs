namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using EuropeanRoad;
using NationalRoad;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

public static class FeatureValidationExtensions
{
    public static ZipArchiveProblems ValidateUniqueIdentifiers<TFeature, TIdentifier>(this ZipArchive archive, List<Feature<TFeature>> features, FeatureType featureType, ExtractFileName fileName, Func<Feature<TFeature>, TIdentifier> getIdentifier)
        where TFeature : class
    {
        var problems = ZipArchiveProblems.None;

        var knownIdentifiers = new Dictionary<TIdentifier, RecordNumber>();

        foreach (var feature in features)
        {
            var identifier = getIdentifier(feature);

            if (knownIdentifiers.TryGetValue(identifier, out var existingRecordNumber))
            {
                var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                problems += recordContext.IdentifierNotUnique(identifier.ToString(), existingRecordNumber);
                continue;
            }

            knownIdentifiers.Add(identifier, feature.RecordNumber);
        }

        return problems;
    }

    public static ZipArchiveProblems ValidateUniqueEuropeanRoads(this ZipArchive archive, List<Feature<EuropeanRoadFeatureCompareAttributes>> features, FeatureType featureType, ExtractFileName fileName)
    {
        return ValidateUniqueRecords(features,
            (item1, item2) => item1.RoadSegmentTempId == item2.RoadSegmentTempId && item1.Number == item2.Number,
            (feature, duplicateFeature) =>
            {
                var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                return recordContext.EuropeanRoadNotUnique(feature.Attributes.Id, duplicateFeature.RecordNumber, duplicateFeature.Attributes.Id);
            });
    }

    public static ZipArchiveProblems ValidateUniqueNationalRoads(this ZipArchive archive, List<Feature<NationalRoadFeatureCompareAttributes>> features, FeatureType featureType, ExtractFileName fileName)
    {
        return ValidateUniqueRecords(features,
            (item1, item2) => item1.RoadSegmentTempId == item2.RoadSegmentTempId && item1.Number == item2.Number,
            (feature, duplicateFeature) =>
        {
            var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
            return recordContext.NationalRoadNotUnique(feature.Attributes.Id, duplicateFeature.RecordNumber, duplicateFeature.Attributes.Id);
        });
    }

    public static ZipArchiveProblems ValidateMissingRoadSegments<T>(this ZipArchive archive, List<Feature<T>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
        where T : RoadSegmentDynamicAttributeAttributes
    {
        var problems = ZipArchiveProblems.None;

        if (!features.Any())
        {
            return problems;
        }

        var featureType = FeatureType.Change;

        foreach (var feature in features
                     .Where(x => x.Attributes.RoadSegmentId > 0
                     && !context.ChangedRoadSegments.ContainsKey(x.Attributes.RoadSegmentId)))
        {
            var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
            problems += recordContext.RoadSegmentMissing(feature.Attributes.RoadSegmentId);
        }

        return problems;
    }

    public static ZipArchiveProblems ValidateRoadSegmentsWithoutAttributes<T>(this ZipArchive archive, List<Feature<T>> features, ExtractFileName fileName, Func<ZipArchiveEntry, RoadSegmentId[], FileProblem> problemBuilder, ZipArchiveFeatureReaderContext context)
        where T : RoadSegmentDynamicAttributeAttributes
    {
        var problems = ZipArchiveProblems.None;

        if (!features.Any() && !context.ChangedRoadSegments.Any())
        {
            return problems;
        }

        var featureType = FeatureType.Change;
        var dbfEntry = archive.FindEntry(featureType.ToDbaseFileName(fileName));
        if (dbfEntry is null)
        {
            return problems;
        }

        var segmentsWithoutAttributes = context.ChangedRoadSegments.Values
            .Where(roadSegment => features.All(attribute => attribute.Attributes.RoadSegmentId != roadSegment.Attributes.TempId))
            .Select(roadSegment => roadSegment.Attributes.TempId)
            .ToArray();

        if (segmentsWithoutAttributes.Any())
        {
            problems += problemBuilder(dbfEntry, segmentsWithoutAttributes);
        }

        return problems;
    }

    public static ZipArchiveProblems TryToFillMissingFromAndToPositions<T>(this ZipArchiveProblems problems, List<Feature<T>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
        where T : RoadSegmentDynamicAttributeAttributes
    {
        var featureType = FeatureType.Change;
        var problemFile = featureType.ToDbaseFileName(fileName);

        var roadSegmentGroups = features
            .Where(x => x.Attributes.RoadSegmentId > 0)
            .GroupBy(x => x.Attributes.RoadSegmentId)
            .ToArray();

        foreach (var roadSegmentGroup in roadSegmentGroups)
        {
            var roadSegmentId = roadSegmentGroup.Key;

            var nullFromPosition = roadSegmentGroup
                .Where(x => x.Attributes.FromPosition == RoadSegmentPositionV2.Zero)
                .ToArray();
            if (nullFromPosition.Length == 1)
            {
                var feature = nullFromPosition.Single();
                if (feature.Attributes.FromPosition == RoadSegmentPositionV2.Zero)
                {
                    problems = problems
                        .Remove(problem =>
                            string.Equals(problem.File, problemFile, StringComparison.InvariantCultureIgnoreCase)
                            && problem.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull)
                            && problem.Parameters.Count == 2
                            && problem.Parameters.Any(x => x.Name == "RecordNumber" && x.Value == feature.RecordNumber.ToString())
                            && problem.Parameters.Any(x => x.Name == "Field" && x.Value == "VANPOS")
                        );
                }
            }

            var nullToPosition = roadSegmentGroup
                .Where(x => x.Attributes.ToPosition == RoadSegmentPositionV2.Zero)
                .ToArray();
            if (nullToPosition.Length == 1)
            {
                var feature = nullToPosition.Single();
                if (feature.Attributes.ToPosition == RoadSegmentPositionV2.Zero)
                {
                    if (context.ChangedRoadSegments.TryGetValue(roadSegmentId, out var roadSegmentFeature)
                        && roadSegmentFeature.Attributes.Geometry is not null)
                    {
                        features[features.IndexOf(feature)] = feature with
                        {
                            Attributes = feature.Attributes with
                            {
                                ToPosition = new RoadSegmentPositionV2(roadSegmentFeature.Attributes.Geometry.Length)
                            }
                        };

                        problems = problems
                            .Remove(problem =>
                                string.Equals(problem.File, problemFile, StringComparison.InvariantCultureIgnoreCase)
                                    && problem.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull)
                                    && problem.Parameters.Count == 2
                                    && problem.Parameters.Any(x => x.Name == "RecordNumber" && x.Value == feature.RecordNumber.ToString())
                                    && problem.Parameters.Any(x => x.Name == "Field" && x.Value == "TOTPOS")
                            )
                            .Remove(problem =>
                                string.Equals(problem.File, problemFile, StringComparison.InvariantCultureIgnoreCase)
                                    && problem.Reason == nameof(DbaseFileProblems.FromPositionEqualToOrGreaterThanToPosition)
                                    && problem.Parameters.Any(x => x.Name == "RecordNumber" && x.Value == feature.RecordNumber.ToString())
                            )
                            ;
                    }
                }
            }
        }

        return problems;
    }
    private static ZipArchiveProblems ValidateUniqueRecords<TAttribute>(List<Feature<TAttribute>> features,
        Func<TAttribute, TAttribute, bool> equalityComparer,
        Func<Feature<TAttribute>, Feature<TAttribute>, FileProblem> problemBuilder)
        where TAttribute : class
    {
        var problems = ZipArchiveProblems.None;

        var knownDuplicateRecordNumbers = new List<RecordNumber>();

        foreach (var feature in features)
        {
            var duplicateFeatures = features
                .Where(x => !x.RecordNumber.Equals(feature.RecordNumber) && equalityComparer(x.Attributes, feature.Attributes))
                .ToList();

            if (duplicateFeatures.Any())
            {
                knownDuplicateRecordNumbers.Add(feature.RecordNumber);

                foreach (var duplicateFeature in duplicateFeatures)
                {
                    if (knownDuplicateRecordNumbers.Contains(duplicateFeature.RecordNumber))
                    {
                        continue;
                    }

                    problems += problemBuilder(feature, duplicateFeature);

                    knownDuplicateRecordNumbers.Add(duplicateFeature.RecordNumber);
                }
            }
        }

        return problems;
    }
}
