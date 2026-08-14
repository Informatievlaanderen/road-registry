namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

// The positions of a lineair gerefereerd attribute form a single run per (side of the) segment: the first record
// starts at 0, every next record starts where the previous one ended, and only the last record may leave totPositie
// open (a null totPositie means up to the end of the segment, as does a null vanPositie mean the start). Every v2
// road segment endpoint that accepts attribute values (create outline, change attributes, change geometry) validates
// them here. The segment length is only known when the request itself carries the geometry, so it is optional;
// without it the trailing position is left to the domain.
internal static class RoadSegmentAttributePositionsValidator
{
    public static List<ValidationFailure> Validate(
        IEnumerable<(double? VanPositie, double? TotPositie)> positions,
        string propertyName,
        double? geometryLength,
        ProblemCode.RoadSegment.DynamicAttributeProblemCodes problemCodes)
    {
        var failures = new List<ValidationFailure>();

        var ordered = positions
            .OrderBy(x => x.VanPositie ?? 0.0)
            .ThenBy(x => x.TotPositie ?? double.MaxValue)
            .ToList();
        if (ordered.Count == 0)
        {
            return failures;
        }

        // Negative positions are already reported by the endpoints themselves; checking coverage against them would
        // only pile confusing failures on top.
        if (ordered.Any(x => x.VanPositie < 0 || x.TotPositie < 0))
        {
            return failures;
        }

        double? previousTo = null; // null = up to the end of the segment
        var isFirst = true;
        foreach (var (from, to) in ordered)
        {
            var fromPosition = from ?? 0.0;

            if (isFirst)
            {
                isFirst = false;
                if (!NearlyEqual(fromPosition, 0.0))
                {
                    AddFailure(failures, propertyName, problemCodes.FromPositionNotEqualToZero,
                        new ProblemParameter("FromPosition", fromPosition.ToRoundedMeasurementString()));
                    return failures;
                }
            }
            else if (previousTo is null)
            {
                // The previous record runs to the end of the segment, so nothing can attach to it: it should have
                // stated where it ends.
                AddFailure(failures, propertyName, problemCodes.FromOrToPositionIsNull);
                return failures;
            }
            else if (!NearlyEqual(fromPosition, previousTo.Value))
            {
                AddFailure(failures, propertyName, problemCodes.NotAdjacent,
                    new ProblemParameter("FromPosition", fromPosition.ToRoundedMeasurementString()),
                    new ProblemParameter("ToPosition", previousTo.Value.ToRoundedMeasurementString()));
                return failures;
            }

            // An open end covers up to the end of the segment, so its length is only known when the geometry is.
            var toPosition = to ?? geometryLength;
            if (toPosition is not null && toPosition.Value - fromPosition < Distances.RoadSegmentDynamicAttributeMinimumLength)
            {
                AddFailure(failures, propertyName, problemCodes.HasLengthOfZero,
                    new ProblemParameter("FromPosition", fromPosition.ToRoundedMeasurementString()),
                    new ProblemParameter("ToPosition", toPosition.Value.ToRoundedMeasurementString()));
            }

            previousTo = to;
        }

        if (previousTo is not null && geometryLength is not null && !NearlyEqual(previousTo.Value, geometryLength.Value))
        {
            AddFailure(failures, propertyName, problemCodes.ToPositionNotEqualToLength,
                new ProblemParameter("ToPosition", previousTo.Value.ToRoundedMeasurementString()),
                new ProblemParameter("Length", geometryLength.Value.ToRoundedMeasurementString()));
        }

        return failures;
    }

    // Sided attributes are two independent runs: what the caller states for 'links' plus 'beide' has to cover the
    // left side, and 'rechts' plus 'beide' the right side.
    public static List<ValidationFailure> ValidateSided(
        IEnumerable<(string? Kant, double? VanPositie, double? TotPositie)> items,
        string propertyName,
        double? geometryLength,
        ProblemCode.RoadSegment.DynamicAttributeProblemCodes problemCodes)
    {
        var failures = new List<ValidationFailure>();

        var list = items.ToList();

        // A record whose kant is missing or unknown is already reported and cannot be placed on a side; checking
        // coverage around the hole it leaves would only add misleading failures.
        var sided = list
            .Where(x => x.Kant is not null && RoadSegmentAttributeSide.CanParseUsingDutchName(x.Kant))
            .Select(x => (Side: RoadSegmentAttributeSide.ParseUsingDutchName(x.Kant!), x.VanPositie, x.TotPositie))
            .ToList();
        if (sided.Count != list.Count)
        {
            return failures;
        }

        // When every record applies to both sides the two runs are identical, and validating them both would report
        // every failure twice.
        var sides = sided.All(x => x.Side == RoadSegmentAttributeSide.Beide)
            ? new[] { RoadSegmentAttributeSide.Links }
            : new[] { RoadSegmentAttributeSide.Links, RoadSegmentAttributeSide.Rechts };
        foreach (var side in sides)
        {
            failures.AddRange(Validate(
                sided
                    .Where(x => x.Side == side || x.Side == RoadSegmentAttributeSide.Beide)
                    .Select(x => (x.VanPositie, x.TotPositie)),
                propertyName, geometryLength, problemCodes));
        }

        return failures;
    }

    // All positions and coordinates are rounded to the centimetre, so two positions are the same position when they
    // land on the same centimetre.
    private static bool NearlyEqual(double a, double b) => a.RoundToCm().Equals(b.RoundToCm());

    // The Dutch problem translators build their message from these parameters by index, so every failure must carry
    // the exact parameters its translator expects - translating a failure without them throws.
    private static void AddFailure(
        List<ValidationFailure> failures,
        string propertyName,
        ProblemCode problemCode,
        params ProblemParameter[] parameters)
    {
        failures.Add(new ValidationFailure(propertyName, problemCode.ToString())
        {
            ErrorCode = problemCode.ToString(),
            CustomState = parameters
        });
    }
}
