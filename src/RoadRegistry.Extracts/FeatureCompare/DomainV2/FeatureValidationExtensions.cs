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
