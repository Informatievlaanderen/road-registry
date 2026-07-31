namespace RoadRegistry.RoadSegment.Flattening;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

// The single engine that flattens a road segment into its smaller sub-segments ("platkloppen"): it splits the segment
// at every position where any dynamic attribute changes and, per sub-range, resolves each attribute's coverage. The
// extract, WmsWfsV2 and PBS derived-wegsegment builders all drive off this engine; only the input adapters (which
// attribute records/coverages they carry) and the output mapping (which record type they produce) differ.
public static class RoadSegmentFlattenEngine
{
    public const double Epsilon = 1e-6;

    private static int SideBoth => RoadSegmentAttributeSide.Beide.Translation.Identifier;

    // A coverage row projected to [From, To], with the trailing coverage snapped to the geometry length.
    public readonly record struct Cover<TRow>(double From, double To, TRow Row) where TRow : class;

    // A flattened sub-range: [From, To] for attribute resolution, and ToActual (clamped to the geometry length for the
    // trailing range) for cutting the sub-geometry.
    public readonly record struct FlatRange(double From, double To, double ToActual);

    // Projects each attribute row onto a coverage whose trailing ToPosition (the track-wide maximum) is replaced by the
    // geometry length, so the union of coverages spans exactly [0, length] regardless of stale/incorrect stored
    // ToPositions (in particular V1 data whose last ToPosition need not match the geometry). Only the genuinely trailing
    // coverage(s) are snapped, so a shorter side-specific coverage is never wrongly extended past a Beide tail that
    // already covers the rest of the segment. The input records are never mutated.
    public static List<Cover<TRow>> Normalize<TRow>(
        IReadOnlyList<TRow> rows, double length,
        Func<TRow, double> getFrom, Func<TRow, double> getTo)
        where TRow : class
    {
        var result = new List<Cover<TRow>>(rows.Count);
        if (rows.Count == 0)
        {
            return result;
        }

        var maxTo = rows.Max(getTo);
        foreach (var r in rows)
        {
            var to = getTo(r);
            result.Add(new Cover<TRow>(getFrom(r), to >= maxTo - Epsilon ? length : to, r));
        }

        return result;
    }

    // The ordered sub-ranges: split at every distinct coverage boundary across all tracks, or a single [0, length]
    // range when there are fewer than two positions. The trailing range's geometry extent is clamped to the length.
    //
    // Boundaries that lie less than a centimetre apart are merged into one split position (positions are only stored
    // to centimetre precision, so anything below that is noise). Without this a sub-centimetre sliver sub-range is
    // emitted whose geometry collapses to a single point once rounded to centimetres.
    public static List<FlatRange> Ranges(double length, params IEnumerable<(double From, double To)>[] tracks)
    {
        var positions = new SortedSet<double>();
        foreach (var track in tracks)
        {
            foreach (var (from, to) in track)
            {
                positions.Add(from);
                positions.Add(to);
            }
        }

        if (positions.Count < 2)
        {
            positions.Clear();
            positions.Add(0);
            positions.Add(length);
        }

        var ordered = Merge(positions);
        if (ordered.Count < 2)
        {
            return [];
        }

        var ranges = new List<FlatRange>(ordered.Count - 1);
        for (var i = 1; i < ordered.Count; i++)
        {
            ranges.Add(new FlatRange(ordered[i - 1], ordered[i], i < ordered.Count - 1 ? ordered[i] : length));
        }

        return ranges;
    }

    // Collapses split positions that are within the position tolerance of each other, keeping the first of each group.
    private static List<double> Merge(SortedSet<double> positions)
    {
        var merged = new List<double>(positions.Count);
        foreach (var position in positions)
        {
            if (merged.Count == 0 || !merged[^1].IsReasonablyEqualTo(position, DefaultTolerances.GeometryToleranceV2))
            {
                merged.Add(position);
            }
        }

        return merged;
    }

    public static IEnumerable<(double From, double To)> Pairs<TRow>(this List<Cover<TRow>> covers)
        where TRow : class
    {
        return covers.Select(c => (c.From, c.To));
    }

    public static TRow? Resolve<TRow>(List<Cover<TRow>> covers, double from, double to)
        where TRow : class
    {
        return covers.FirstOrDefault(c => c.From <= from + Epsilon && c.To >= to - Epsilon).Row;
    }

    public static TRow? ResolveSided<TRow>(List<Cover<TRow>> covers, double from, double to, int side, Func<TRow, int?> getSide)
        where TRow : class
    {
        return covers.FirstOrDefault(c => c.From <= from + Epsilon && c.To >= to - Epsilon && (getSide(c.Row) == SideBoth || getSide(c.Row) == side)).Row;
    }
}
