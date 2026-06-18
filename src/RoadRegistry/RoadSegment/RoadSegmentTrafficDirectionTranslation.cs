namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public static class RoadSegmentTrafficDirectionTranslation
{
    public static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> ToTrafficDirection(
        RoadSegmentDynamicAttributeValues<bool> forward,
        RoadSegmentDynamicAttributeValues<bool> backward)
    {
        var values = new List<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, RoadSegmentTrafficDirection Value)>();

        var sides = forward.Values.Select(x => x.Side)
            .Concat(backward.Values.Select(x => x.Side))
            .Distinct();

        foreach (var side in sides)
        {
            var forwardValues = forward.Values.Where(x => x.Side == side).ToList();
            var backwardValues = backward.Values.Where(x => x.Side == side).ToList();

            var breakpoints = forwardValues.Select(x => x.Coverage.From)
                .Concat(forwardValues.Select(x => x.Coverage.To))
                .Concat(backwardValues.Select(x => x.Coverage.From))
                .Concat(backwardValues.Select(x => x.Coverage.To))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            for (var i = 0; i + 1 < breakpoints.Count; i++)
            {
                var from = breakpoints[i];
                var to = breakpoints[i + 1];

                var direction = RoadSegmentTrafficDirection.FromAccess(
                    AccessAt(forwardValues, from, to),
                    AccessAt(backwardValues, from, to));

                values.Add((new RoadSegmentPositionCoverage(from, to), side, direction));
            }
        }

        return new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(values);
    }

    public static RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> ToPedestrianTrafficDirection(
        RoadSegmentDynamicAttributeValues<bool> access)
    {
        return new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(
            access.Values.Select(x => (x.Coverage, x.Side, RoadSegmentPedestrianTrafficDirection.FromAccess(x.Value))));
    }

    public static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? ToTrafficDirectionOrNull(
        RoadSegmentDynamicAttributeValues<bool>? forward,
        RoadSegmentDynamicAttributeValues<bool>? backward)
    {
        return forward is not null && backward is not null ? ToTrafficDirection(forward, backward) : null;
    }

    public static RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? ToPedestrianTrafficDirectionOrNull(
        RoadSegmentDynamicAttributeValues<bool>? access)
    {
        return access is not null ? ToPedestrianTrafficDirection(access) : null;
    }

    public static RoadSegmentDynamicAttributeValues<bool> ToForwardAccess(RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> direction)
    {
        return new RoadSegmentDynamicAttributeValues<bool>(
            direction.Values.Select(x => (x.Coverage, x.Side, x.Value is not null && x.Value.IsForwardAccessAllowed)));
    }

    public static RoadSegmentDynamicAttributeValues<bool> ToBackwardAccess(RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> direction)
    {
        return new RoadSegmentDynamicAttributeValues<bool>(
            direction.Values.Select(x => (x.Coverage, x.Side, x.Value is not null && x.Value.IsBackwardAccessAllowed)));
    }

    public static RoadSegmentDynamicAttributeValues<bool> ToPedestrianAccess(RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> direction)
    {
        return new RoadSegmentDynamicAttributeValues<bool>(
            direction.Values.Select(x => (x.Coverage, x.Side, x.Value is not null && x.Value.IsAccessAllowed)));
    }

    /// <summary>
    /// Resolves an updated traffic-direction attribute from a partial boolean change (forward/backward may
    /// be null when unchanged), falling back to the existing direction for the side that is not provided.
    /// Returns null when neither side changes.
    /// </summary>
    public static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? Resolve(
        RoadSegmentDynamicAttributeValues<bool>? forward,
        RoadSegmentDynamicAttributeValues<bool>? backward,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> existing)
    {
        if (forward is null && backward is null)
        {
            return null;
        }

        return ToTrafficDirection(forward ?? ToForwardAccess(existing), backward ?? ToBackwardAccess(existing));
    }

    public static RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? ResolvePedestrian(
        RoadSegmentDynamicAttributeValues<bool>? access)
    {
        return access is null ? null : ToPedestrianTrafficDirection(access);
    }

    private static bool AccessAt(IEnumerable<RoadSegmentDynamicAttributeValue<bool>> values, RoadSegmentPositionV2 from, RoadSegmentPositionV2 to)
    {
        var match = values.FirstOrDefault(x => x.Coverage.From <= from && x.Coverage.To >= to);
        return match?.Value ?? false;
    }
}
