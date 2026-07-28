namespace RoadRegistry.WmsWfsV2.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using Schema.Records;

// Flattens a road segment into AfgeleideWegsegmenten rows: the segment is split at every position where any dynamic
// attribute changes, and for each sub-range the attributes are resolved to plain values and a sub-geometry is cut from
// the segment geometry. Works off the per-attribute Att records (which carry VANPOS/TOTPOS/code/label/KANT), so it is
// driven by exactly the same data the projection stores in the dynamic-attribute tables.
internal static class DerivedWegsegmentFlattener
{
    private const double Epsilon = 1e-6;

    private static int SideBoth => RoadSegmentAttributeSide.Beide.Translation.Identifier;
    private static int SideLeft => RoadSegmentAttributeSide.Links.Translation.Identifier;
    private static int SideRight => RoadSegmentAttributeSide.Rechts.Translation.Identifier;

    public static List<DerivedRoadSegmentRecord> Flatten(
        int segId,
        Geometry geometry,
        int? status, string? lblStatus, int? method, string? lblMethod, int? beginNodeId, int? endNodeId,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord> morphology,
        IReadOnlyList<RoadSegmentCategoryAttributeRecord> category,
        IReadOnlyList<RoadSegmentAccessRestrictionAttributeRecord> access,
        IReadOnlyList<RoadSegmentSurfaceTypeAttributeRecord> surface,
        IReadOnlyList<RoadSegmentStreetNameAttributeRecord> streetName,
        IReadOnlyList<RoadSegmentMaintenanceAuthorityAttributeRecord> maintainer,
        IReadOnlyList<RoadSegmentCarTrafficDirectionAttributeRecord> car,
        IReadOnlyList<RoadSegmentBikeTrafficDirectionAttributeRecord> bike,
        IReadOnlyList<RoadSegmentPedestrianTrafficDirectionAttributeRecord> pedestrian,
        string? euNummers, string? nwNummers,
        DateTimeOffset creatie, DateTimeOffset versie)
    {
        var length = geometry.Length;

        // In (particularly V1) data the last dynamic-attribute ToPosition does not always line up with the actual
        // geometry length — it can be shorter or longer (e.g. after a geometry change that only rewrote some
        // attributes). Snap each attribute's trailing coverage (per side for sided attributes) to the geometry
        // length so the split positions and resolution cover exactly [0, length]. This mirrors the extracts
        // RoadSegmentFlattener/UseGeometryLengthIfPositionIsLast handling.
        var morphologyCov = NormalizeToLength(morphology, length, x => x.VANPOS, x => x.TOTPOS);
        var categoryCov = NormalizeToLength(category, length, x => x.VANPOS, x => x.TOTPOS);
        var accessCov = NormalizeToLength(access, length, x => x.VANPOS, x => x.TOTPOS);
        var surfaceCov = NormalizeToLength(surface, length, x => x.VANPOS, x => x.TOTPOS);
        var streetNameCov = NormalizeToLength(streetName, length, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
        var maintainerCov = NormalizeToLength(maintainer, length, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
        var carCov = NormalizeToLength(car, length, x => x.VANPOS, x => x.TOTPOS);
        var bikeCov = NormalizeToLength(bike, length, x => x.VANPOS, x => x.TOTPOS);
        var pedestrianCov = NormalizeToLength(pedestrian, length, x => x.VANPOS, x => x.TOTPOS);

        var positions = new SortedSet<double>();
        void Add<TRow>(List<Cover<TRow>> covers) where TRow : class
        {
            foreach (var c in covers) { positions.Add(c.From); positions.Add(c.To); }
        }
        Add(morphologyCov);
        Add(categoryCov);
        Add(accessCov);
        Add(surfaceCov);
        Add(streetNameCov);
        Add(maintainerCov);
        Add(carCov);
        Add(bikeCov);
        Add(pedestrianCov);

        if (positions.Count < 2)
        {
            positions.Clear();
            positions.Add(0);
            positions.Add(length);
        }

        var ordered = positions.ToList();
        var lengthIndexedLine = new LengthIndexedLine(geometry);
        var result = new List<DerivedRoadSegmentRecord>();

        for (var i = 1; i < ordered.Count; i++)
        {
            var from = ordered[i - 1];
            var to = ordered[i];
            var toActual = i < ordered.Count - 1 ? to : length;

            var m = Resolve(morphologyCov, from, to);
            var c = Resolve(categoryCov, from, to);
            var a = Resolve(accessCov, from, to);
            var s = Resolve(surfaceCov, from, to);
            var lStr = ResolveSided(streetNameCov, from, to, SideLeft, x => x.KANT);
            var rStr = ResolveSided(streetNameCov, from, to, SideRight, x => x.KANT);
            var lBeh = ResolveSided(maintainerCov, from, to, SideLeft, x => x.KANT);
            var rBeh = ResolveSided(maintainerCov, from, to, SideRight, x => x.KANT);
            var carR = Resolve(carCov, from, to);
            var bikeR = Resolve(bikeCov, from, to);
            var pedR = Resolve(pedestrianCov, from, to);

            result.Add(new DerivedRoadSegmentRecord
            {
                WS_OIDN = segId,
                STATUS = status,
                LBLSTATUS = lblStatus,
                METHODE = method,
                LBLMETHODE = lblMethod,
                B_WK_OIDN = beginNodeId,
                E_WK_OIDN = endNodeId,
                MORF = m?.MORF,
                LBLMORF = m?.LBLMORF,
                WEGCAT = c?.WEGCAT,
                LBLWEGCAT = c?.LBLWEGCAT,
                TOEGANG = a?.TOEGANG,
                LBLTOEGANG = a?.LBLTOEGANG,
                VERHARDING = s?.VERHARDING,
                LBLVERHARD = s?.LBLVERHARD,
                LSTRNMID = lStr?.STRTNMID,
                RSTRNMID = rStr?.STRTNMID,
                LBEHEER = lBeh?.BEHEER,
                RBEHEER = rBeh?.BEHEER,
                VERKEERSTYPE_AUTO = carR?.RICHTING,
                LBLVERKEERSTYPE_AUTO = TrafficDirectionLabel(carR?.RICHTING),
                VERKEERSTYPE_FIETS = bikeR?.RICHTING,
                LBLVERKEERSTYPE_FIETS = TrafficDirectionLabel(bikeR?.RICHTING),
                VERKEERSTYPE_VOETGANGER = pedR?.RICHTING,
                LBLVERKEERSTYPE_VOETGANGER = PedestrianTrafficDirectionLabel(pedR?.RICHTING),
                EUNUMMERS = euNummers,
                NWNUMMERS = nwNummers,
                GEOMETRIE = lengthIndexedLine.ExtractLine(from, toActual).RoundToCm(),
                CREATIE = creatie,
                VERSIE = versie
            });
        }

        return result;
    }

    // A coverage row with its ToPosition already snapped to the geometry length where it is the trailing one.
    private readonly record struct Cover<TRow>(double From, double To, TRow Row) where TRow : class;

    // Projects each attribute row onto a coverage whose trailing ToPosition (the maximum, computed per side for
    // sided attributes) is replaced by the geometry length, so the union of coverages spans exactly [0, length]
    // regardless of stale/incorrect stored ToPositions. The input records are never mutated.
    private static List<Cover<TRow>> NormalizeToLength<TRow>(
        IReadOnlyList<TRow> rows, double length,
        Func<TRow, double> getFrom, Func<TRow, double> getTo, Func<TRow, int?>? getSide = null)
        where TRow : class
    {
        var result = new List<Cover<TRow>>(rows.Count);
        if (rows.Count == 0)
        {
            return result;
        }

        if (getSide is null)
        {
            var maxTo = rows.Max(getTo);
            foreach (var r in rows)
            {
                var to = getTo(r);
                result.Add(new Cover<TRow>(getFrom(r), to >= maxTo - Epsilon ? length : to, r));
            }
        }
        else
        {
            foreach (var group in rows.GroupBy(getSide))
            {
                var maxTo = group.Max(getTo);
                foreach (var r in group)
                {
                    var to = getTo(r);
                    result.Add(new Cover<TRow>(getFrom(r), to >= maxTo - Epsilon ? length : to, r));
                }
            }
        }

        return result;
    }

    private static TRow? Resolve<TRow>(List<Cover<TRow>> covers, double from, double to)
        where TRow : class
    {
        return covers.FirstOrDefault(c => c.From <= from + Epsilon && c.To >= to - Epsilon).Row;
    }

    private static TRow? ResolveSided<TRow>(List<Cover<TRow>> covers, double from, double to, int side, Func<TRow, int?> getSide)
        where TRow : class
    {
        return covers.FirstOrDefault(c => c.From <= from + Epsilon && c.To >= to - Epsilon && (getSide(c.Row) == SideBoth || getSide(c.Row) == side)).Row;
    }

    // The RICHTING attribute is a coded int; resolve it to its Dutch label ("heen"/"terug"/"beide"/"geen") via the
    // metadata type. Car and bike use RoadSegmentTrafficDirection; pedestrians use RoadSegmentPedestrianTrafficDirection.
    private static string? TrafficDirectionLabel(int? richting) => richting is null
        ? null
        : Array.Find(RoadSegmentTrafficDirection.All, x => x.Translation.Identifier == richting)?.Translation.Name;
    private static string? PedestrianTrafficDirectionLabel(int? richting) => richting is null
        ? null
        : Array.Find(RoadSegmentPedestrianTrafficDirection.All, x => x.Translation.Identifier == richting)?.Translation.Name;
}
