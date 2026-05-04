namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using EuropeanRoad;
using NationalRoad;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

public static class FeatureValidationExtensions
{
    public static ZipArchiveProblems ValidateGeometryIsAtLeastPartiallyWithinTransactionZone(this ExtractGeometry extractGeometry, Geometry geometry, IShapeFileRecordProblemBuilder builder)
    {
        if (extractGeometry.Value.Disjoint(geometry))
        {
            return ZipArchiveProblems.Single(builder.ShapeRecordGeometryIsOutsideTransactionZone());
        }

        return ZipArchiveProblems.None;
    }

    public static ZipArchiveProblems ValidateUniqueIdentifiers<TFeature, TIdentifier>(this ZipArchive archive, List<Feature<TFeature>> features, FeatureType featureType, ExtractFileName fileName, string identifierField, Func<Feature<TFeature>, TIdentifier> getIdentifier)
        where TFeature : class
    {
        var problems = ZipArchiveProblems.None;

        var knownIdentifiers = new Dictionary<TIdentifier, RecordNumber>();

        foreach (var feature in features)
        {
            var identifier = getIdentifier(feature);

            if (knownIdentifiers.TryGetValue(identifier, out var existingRecordNumber))
            {
                var recordContext = fileName.AtDbaseRecord(featureType, feature.RecordNumber)
                    .WithIdentifier(identifierField, identifier.ToString());
                problems += recordContext.IdentifierNotUnique(identifier.ToString(), existingRecordNumber);
                continue;
            }

            knownIdentifiers.Add(identifier, feature.RecordNumber);
        }

        return problems;
    }

    public static ZipArchiveProblems ValidateUniqueGradeSeparatedJunctions(this ZipArchive archive, List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>> features, FeatureType featureType, ExtractFileName fileName)
    {
        return ValidateUniqueRecords(features,
            (item1, item2) =>
                (item1.LowerRoadSegmentTempId == item2.LowerRoadSegmentTempId && item1.UpperRoadSegmentTempId == item2.UpperRoadSegmentTempId)
                ||
                (item1.LowerRoadSegmentTempId == item2.UpperRoadSegmentTempId && item1.UpperRoadSegmentTempId == item2.LowerRoadSegmentTempId),
            (feature, duplicateFeature) =>
        {
            var recordContext = fileName
                .AtDbaseRecord(featureType, feature.RecordNumber)
                .WithIdentifier(nameof(GradeSeparatedJunctionDbaseRecord.OK_OIDN), feature.Attributes.Id);
            return recordContext.GradeSeparatedJunctionNotUnique(feature.Attributes.Id, duplicateFeature.Attributes.Id);
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
