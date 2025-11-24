namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using BackOffice.Core.ProblemCodes;
using BackOffice.Extensions;
using NetTopologySuite.Geometries;
using ValueObjects;

public static class ValidationExtensions
{
    public static Problems Validate<T>(this RoadSegmentDynamicAttributeValues<T> attributeValues,
        RoadSegmentId roadSegmentId,
        double segmentLength,
        ProblemCode.RoadSegment.DynamicAttributeProblemCodes problemCodes)
    {
        var problems = Problems.None;

        if (!attributeValues.Values.Any())
        {
            //TODO-pr mag dit wel? moet dit geen fout geven? -> tbd met Mathieu aftoetsen
            return problems;
        }

        var sortedAttributes = attributeValues.Values
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Side)
            .ToList();

        var valuesGroupedByPositionSegment = sortedAttributes
            .ToLookup(x => (x.From, x.To), x => (x.Side, x.Value));

        //TODO-pr nakijken of het hier gegarandeerd is dat altijd het volledige wegsegment is verantwoordt (rekening houdende met null from/to pos) + add test indien niet het geval

        // ensure each position segment has correct amount of values per side
        RoadSegmentPosition? previousToPosition = null;
        foreach (var group in valuesGroupedByPositionSegment)
        {
            if ((group.Key.From is null && group.Key.To is not null)
                || (group.Key.From is not null && group.Key.To is null))
            {
                problems += new Error(problemCodes.FromOrToPositionIsNull,
                    new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                    new ProblemParameter("FromPosition", group.Key.From?.ToString() ?? string.Empty),
                    new ProblemParameter("ToPosition", group.Key.To?.ToString() ?? string.Empty));
                continue;
            }

            if (group.Key.From is not null && group.Key.To is not null)
            {
                if (previousToPosition is null)
                {
                    if (group.Key.From != RoadSegmentPosition.Zero)
                    {
                        problems += new Error(problemCodes.FromPositionNotEqualToZero,
                            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                            new ProblemParameter("FromPosition", group.Key.From.Value.ToString()));
                    }
                }
                else
                {
                    if (group.Key.From != previousToPosition.Value)
                    {
                        problems += new Error(problemCodes.NotAdjacent,
                            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                            new ProblemParameter("ToPosition", previousToPosition.Value.ToString()),
                            new ProblemParameter("FromPosition", group.Key.From.Value.ToString()));
                    }

                    if (group.Key.From == group.Key.To)
                    {
                        problems += new Error(problemCodes.HasLengthOfZero,
                            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                            new ProblemParameter("FromPosition", group.Key.From.Value.ToString()),
                            new ProblemParameter("ToPosition", group.Key.To.Value.ToString()));
                    }
                }

                previousToPosition = group.Key.To;
            }

            var both = group.Where(x => x.Side == RoadSegmentAttributeSide.Both).ToList();
            var notBoth = group.Where(x => x.Side != RoadSegmentAttributeSide.Both).ToList();

            if (both.Count > 1)
            {
                problems += new Error(problemCodes.ValueNotUniqueWithinSegment,
                    new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                    new ProblemParameter("FromPosition", group.Key.From?.ToString() ?? string.Empty),
                    new ProblemParameter("ToPosition", group.Key.To?.ToString() ?? string.Empty));
            }
            else if (both.Count == 1 && notBoth.Count > 0)
            {
                problems += new Error(problemCodes.LeftOrRightNotAllowedWhenUsingBoth,
                    new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                    new ProblemParameter("FromPosition", group.Key.From?.ToString() ?? string.Empty),
                    new ProblemParameter("ToPosition", group.Key.To?.ToString() ?? string.Empty));
            }
            else if (both.Count == 0)
            {
                var hasNotUniqueRecords = notBoth
                    .GroupBy(x => x.Side)
                    .Any(x => x.Count() > 1);
                if (hasNotUniqueRecords)
                {
                    problems += new Error(problemCodes.ValueNotUniqueWithinSegment,
                        new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                        new ProblemParameter("FromPosition", group.Key.From?.ToString() ?? string.Empty),
                        new ProblemParameter("ToPosition", group.Key.To?.ToString() ?? string.Empty));
                }
            }
        }

        if (previousToPosition is not null && !previousToPosition.Value.ToDouble().IsReasonablyEqualTo(segmentLength))
        {
            problems += new Error(problemCodes.ToPositionNotEqualToLength,
                new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
                new ProblemParameter("ToPosition", previousToPosition.Value.ToString()),
                new ProblemParameter("Length", segmentLength.ToRoundedMeasurementString()));
        }

        var valuesOnEntireSegment = valuesGroupedByPositionSegment[(null, null)];
        if (valuesOnEntireSegment.Any() && valuesGroupedByPositionSegment.Count > 1)
        {
            problems += new Error(problemCodes.AnotherSegmentFoundBesidesTheGlobalSegment,
                new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()));
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
