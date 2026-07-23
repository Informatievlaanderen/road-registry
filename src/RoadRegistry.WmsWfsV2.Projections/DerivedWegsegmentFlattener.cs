namespace RoadRegistry.WmsWfsV2.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Infrastructure.Dbase;
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

        var positions = new SortedSet<double>();
        void Add(double from, double to) { positions.Add(from); positions.Add(to); }
        foreach (var r in morphology) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in category) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in access) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in surface) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in streetName) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in maintainer) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in car) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in bike) Add(r.VANPOS, r.TOTPOS);
        foreach (var r in pedestrian) Add(r.VANPOS, r.TOTPOS);

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

            var m = Resolve(morphology, from, to, x => x.VANPOS, x => x.TOTPOS);
            var c = Resolve(category, from, to, x => x.VANPOS, x => x.TOTPOS);
            var a = Resolve(access, from, to, x => x.VANPOS, x => x.TOTPOS);
            var s = Resolve(surface, from, to, x => x.VANPOS, x => x.TOTPOS);
            var lStr = ResolveSided(streetName, from, to, SideLeft, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
            var rStr = ResolveSided(streetName, from, to, SideRight, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
            var lBeh = ResolveSided(maintainer, from, to, SideLeft, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
            var rBeh = ResolveSided(maintainer, from, to, SideRight, x => x.VANPOS, x => x.TOTPOS, x => x.KANT);
            var carR = Resolve(car, from, to, x => x.VANPOS, x => x.TOTPOS);
            var bikeR = Resolve(bike, from, to, x => x.VANPOS, x => x.TOTPOS);
            var pedR = Resolve(pedestrian, from, to, x => x.VANPOS, x => x.TOTPOS);

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
                AUTOHEEN = Heen(carR?.RICHTING),
                AUTOTERUG = Terug(carR?.RICHTING),
                FIETSHEEN = Heen(bikeR?.RICHTING),
                FIETSTERUG = Terug(bikeR?.RICHTING),
                VOETGANGER = Voetganger(pedR?.RICHTING),
                EUNUMMERS = euNummers,
                NWNUMMERS = nwNummers,
                GEOMETRIE = lengthIndexedLine.ExtractLine(from, toActual).RoundToCm(),
                CREATIE = creatie,
                VERSIE = versie
            });
        }

        return result;
    }

    private static TRow Resolve<TRow>(IReadOnlyList<TRow> rows, double from, double to, Func<TRow, double> getFrom, Func<TRow, double> getTo)
        where TRow : class
    {
        return rows.FirstOrDefault(r => getFrom(r) <= from + Epsilon && getTo(r) >= to - Epsilon);
    }

    private static TRow ResolveSided<TRow>(IReadOnlyList<TRow> rows, double from, double to, int side, Func<TRow, double> getFrom, Func<TRow, double> getTo, Func<TRow, int?> getSide)
        where TRow : class
    {
        return rows.FirstOrDefault(r => getFrom(r) <= from + Epsilon && getTo(r) >= to - Epsilon && (getSide(r) == SideBoth || getSide(r) == side));
    }

    private static int? Heen(int? richting) => richting is null
        ? null
        : (richting == RoadSegmentTrafficDirection.Forward.Translation.Identifier || richting == RoadSegmentTrafficDirection.Both.Translation.Identifier).ToDbaseShortValue();
    private static int? Terug(int? richting) => richting is null
        ? null
        : (richting == RoadSegmentTrafficDirection.Backward.Translation.Identifier || richting == RoadSegmentTrafficDirection.Both.Translation.Identifier).ToDbaseShortValue();
    private static int? Voetganger(int? richting) => richting is null
        ? null
        : (richting == RoadSegmentTrafficDirection.Both.Translation.Identifier).ToDbaseShortValue();
}
