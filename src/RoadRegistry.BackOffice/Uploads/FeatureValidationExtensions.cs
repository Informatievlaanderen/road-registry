using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureCompare.Translators;

namespace RoadRegistry.BackOffice.Uploads
{
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Extensions;
    using Extracts.Dbase.RoadSegments;

    public static class FeatureValidationExtensions
    {
        public static ZipArchiveProblems ValidateProjectionFile(this ZipArchive archive, FeatureType featureType, ExtractFileName fileName, Encoding encoding)
        {
            var prjFileName = featureType.ToProjectionFileName(fileName);
            var prjEntry = archive.FindEntry(prjFileName);
            if (prjEntry is null)
            {
                return ZipArchiveProblems.None
                    .RequiredFileMissing(prjFileName);
            }

            return new ZipArchiveProjectionFormatEntryValidator(encoding)
                .Validate(prjEntry, ZipArchiveValidationContext.Empty).Item1;
        }

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
                (item1, item2) => item1.RoadSegmentId == item2.RoadSegmentId && item1.Number == item2.Number,
                (feature, duplicateFeature) =>
                {
                    var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                    return recordContext.EuropeanRoadNotUnique(feature.Attributes.Id, duplicateFeature.RecordNumber, duplicateFeature.Attributes.Id);
                });
        }

        public static ZipArchiveProblems ValidateUniqueNationalRoads(this ZipArchive archive, List<Feature<NationalRoadFeatureCompareAttributes>> features, FeatureType featureType, ExtractFileName fileName)
        {
            return ValidateUniqueRecords(features,
                (item1, item2) => item1.RoadSegmentId == item2.RoadSegmentId && item1.Number == item2.Number,
                (feature, duplicateFeature) =>
            {
                var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                return recordContext.NationalRoadNotUnique(feature.Attributes.Id, duplicateFeature.RecordNumber, duplicateFeature.Attributes.Id);
            });
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

        public static ZipArchiveProblems ValidateMissingRoadSegments<T>(this ZipArchive archive, List<Feature<T>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
            where T: RoadSegmentAttributeFeatureCompareAttributes
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

        public static ZipArchiveProblems ValidateMissingRoadNodes(this ZipArchive archive, List<Feature<RoadSegmentFeatureCompareAttributes>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
        {
            var problems = ZipArchiveProblems.None;

            if (!features.Any())
            {
                return problems;
            }

            var featureType = FeatureType.Change;

            foreach (var feature in features)
            {
                if (feature.Attributes.StartNodeId > 0 && !context.ChangedRoadNodes.ContainsKey(feature.Attributes.StartNodeId))
                {
                    var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                    problems += recordContext.RoadSegmentStartNodeMissing(feature.Attributes.StartNodeId);
                }

                if (feature.Attributes.EndNodeId > 0 && !context.ChangedRoadNodes.ContainsKey(feature.Attributes.EndNodeId))
                {
                    var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                    problems += recordContext.RoadSegmentEndNodeMissing(feature.Attributes.EndNodeId);
                }
            }

            return problems;
        }

        public static ZipArchiveProblems ValidateRoadSegmentsWithoutAttributes<T>(this ZipArchive archive, List<Feature<T>> features, ExtractFileName fileName, Func<ZipArchiveEntry, RoadSegmentId[], FileProblem> problemBuilder, ZipArchiveFeatureReaderContext context)
            where T: RoadSegmentAttributeFeatureCompareAttributes
        {
            var problems = ZipArchiveProblems.None;

            if (!features.Any())
            {
                return problems;
            }

            var featureType = FeatureType.Change;

            var segmentsWithoutAttributes = context.ChangedRoadSegments.Values
                .Where(roadSegment => features.All(attribute => attribute.Attributes.RoadSegmentId != roadSegment.Attributes.Id))
                .Select(roadSegment => roadSegment.Attributes.Id)
                .ToArray();

            if (segmentsWithoutAttributes.Any())
            {
                var dbfEntry = archive.FindEntry(featureType.ToDbaseFileName(fileName));
                problems += problemBuilder(dbfEntry, segmentsWithoutAttributes);
            }

            return problems;
        }

        public static ZipArchiveProblems TryToFillMissingFromAndToPositions<T>(this ZipArchiveProblems problems, List<Feature<T>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
            where T : RoadSegmentAttributeFeatureCompareAttributes
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
                    .Where(x => x.Attributes.FromPosition == RoadSegmentPosition.Zero)
                    .ToArray();
                if (nullFromPosition.Length == 1)
                {
                    var feature = nullFromPosition.Single();
                    if (feature.Attributes.FromPosition == RoadSegmentPosition.Zero)
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
                    .Where(x => x.Attributes.ToPosition == RoadSegmentPosition.Zero)
                    .ToArray();
                if (nullToPosition.Length == 1)
                {
                    var feature = nullToPosition.Single();
                    if (feature.Attributes.ToPosition == RoadSegmentPosition.Zero)
                    {
                        if (context.ChangedRoadSegments.TryGetValue(roadSegmentId, out var roadSegmentFeature)
                            && roadSegmentFeature.Attributes.Geometry is not null)
                        {
                            features[features.IndexOf(feature)] = feature with
                            {
                                Attributes = feature.Attributes with
                                {
                                    ToPosition = RoadSegmentPosition.FromDouble(roadSegmentFeature.Attributes.Geometry.Length)
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
    }
}
