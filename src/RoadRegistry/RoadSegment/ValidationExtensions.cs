namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using BackOffice.Core.ProblemCodes;
using NetTopologySuite.Geometries;
using ValueObjects;

public static class ValidationExtensions
{
    public static Problems Validate<T>(this RoadSegmentDynamicAttributeValues<T>? attributeValues,
        RoadSegmentId roadSegmentId,
        string attributeName,
        double segmentLength)
    {
        var problems = Problems.None;

        if (attributeValues is null || !attributeValues.Values.Any())
        {
            return problems;
        }

        var sortedAttributes = attributeValues.Values
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Side)
            .ToList();

        var valuesGroupedByPositionSegment = sortedAttributes
            .ToLookup(x => (x.From, x.To), x => (x.Side, x.Value));

        // ensure each position segment has correct amount of values per side
        RoadSegmentPosition? previousToPosition = null;
        foreach (var group in valuesGroupedByPositionSegment)
        {
            if ((group.Key.From is null && group.Key.To is not null)
                || (group.Key.From is not null && group.Key.To is null))
            {
                // invalid position segment, both from/to must be null or not null
                problems += new Error("ProblemCode.RoadSegment.DynamicAttribute.TODO-pr");
                continue;
            }

            if (group.Key.From is not null && group.Key.To is not null)
            {
                if (previousToPosition is null)
                {
                    if (group.Key.From != RoadSegmentPosition.Zero)
                    {
                        problems += new RoadSegmentDynamicAttributeFromPositionNotEqualToZero(
                            roadSegmentId,
                            attributeName,
                            group.Key.From.Value);
                    }
                }
                else
                {
                    if (group.Key.From != previousToPosition.Value)
                    {
                        problems += new RoadSegmentDynamicAttributesNotAdjacent(
                            roadSegmentId,
                            attributeName,
                            previousToPosition.Value,
                            group.Key.From.Value);
                    }

                    if (group.Key.From == group.Key.To)
                    {
                        problems += new RoadSegmentDynamicAttributeHasLengthOfZero(
                            roadSegmentId,
                            attributeName,
                            group.Key.From.Value,
                            group.Key.To.Value);
                    }
                }

                previousToPosition = group.Key.To;
            }

            var both = group.Where(x => x.Side == RoadSegmentAttributeSide.Both).ToList();
            var notBoth = group.Where(x => x.Side != RoadSegmentAttributeSide.Both).ToList();

            if (both.Count > 1)
            {
                // only 1 value allowed per unique segment on the roadsegment
                problems += new Error("ProblemCode.RoadSegment.DynamicAttribute.TODO-pr");
            }
            else if (both.Count == 1 && notBoth.Count > 0)
            {
                // when both is used, then can't use left/right for the same segment
                problems += new Error("ProblemCode.RoadSegment.DynamicAttribute.TODO-pr");
            }
            else if (both.Count == 0)
            {
                var hasNotUniqueRecords = notBoth
                    .GroupBy(x => x.Side)
                    .Any(x => x.Count() > 1);
                if (hasNotUniqueRecords)
                {
                    // can't use multiple attributes for left/right
                    problems += new Error("ProblemCode.RoadSegment.DynamicAttribute.TODO-pr");
                }
            }
        }

        if (previousToPosition is not null && !previousToPosition.Value.ToDouble().IsReasonablyEqualTo(segmentLength))
        {
            problems += new RoadSegmentDynamicAttributeToPositionNotEqualToLength(
                roadSegmentId,
                attributeName,
                previousToPosition.Value,
                segmentLength);
        }

        var valuesOnEntireSegment = valuesGroupedByPositionSegment[(null, null)];
        if (valuesOnEntireSegment.Any() && valuesGroupedByPositionSegment.Count > 1)
        {
            // only 1 position segment allowed when segment covers entire segment
            return problems.Add(new Error("ProblemCode.RoadSegment.DynamicAttribute.TODO-pr"));
        }

        return problems;
    }

    public static Problems ValidateCollectionMustBeUnique<T>(this IReadOnlyCollection<T>? collection,
        RoadSegmentId roadSegmentId,
        ProblemCode notUniqueProblemCode)
    {
        if (collection is not null && collection.Count != collection.Distinct().Count())
        {
            return Problems.Single(new Error(notUniqueProblemCode, new ProblemParameter("RoadSegmentId", roadSegmentId.ToString())));
        }

        return Problems.None;
    }
}
