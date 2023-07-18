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

    public static class FeatureValidationExtensions
    {
        public static ZipArchiveProblems ValidateProjectionFile(this ZipArchive archive, FeatureType featureType, ExtractFileName fileName, Encoding encoding)
        {
            var prjFileName = featureType.GetProjectionFileName(fileName);
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
                         && !context.KnownRoadSegments.ContainsKey(x.Attributes.RoadSegmentId)))
            {
                var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber);
                problems += recordContext.RoadSegmentMissing(feature.Attributes.RoadSegmentId);
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

            var segmentsWithoutAttributes = context.KnownRoadSegments.Values
                .Where(roadSegment => features.All(attribute => attribute.Attributes.RoadSegmentId != roadSegment.Attributes.Id))
                .Select(roadSegment => roadSegment.Attributes.Id)
                .ToArray();

            if (segmentsWithoutAttributes.Any())
            {
                var dbfEntry = archive.FindEntry(featureType.GetDbaseFileName(fileName));
                problems += problemBuilder(dbfEntry, segmentsWithoutAttributes);
            }

            return problems;
        }
    }
}
