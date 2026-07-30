namespace RoadRegistry.WmsWfsV2.Projections;

using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Flattening;
using RoadRegistry.RoadSegment.ValueObjects;
using Schema.Records;
using static RoadRegistry.RoadSegment.Flattening.RoadSegmentFlattenEngine;

// Flattens a road segment into AfgeleideWegsegmenten rows: the segment is split at every position where any dynamic
// attribute changes, and for each sub-range the attributes are resolved to plain values and a sub-geometry is cut from
// the segment geometry. Works off the per-attribute Att records (which carry VANPOS/TOTPOS/code/label/KANT). The
// split/normalize/resolve algorithm is shared with the extract and PBS flatteners via RoadSegmentFlattenEngine.
internal static class DerivedWegsegmentFlattener
{
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

        var morphologyCov = Normalize(morphology, length, x => x.VANPOS, x => x.TOTPOS);
        var categoryCov = Normalize(category, length, x => x.VANPOS, x => x.TOTPOS);
        var accessCov = Normalize(access, length, x => x.VANPOS, x => x.TOTPOS);
        var surfaceCov = Normalize(surface, length, x => x.VANPOS, x => x.TOTPOS);
        var streetNameCov = Normalize(streetName, length, x => x.VANPOS, x => x.TOTPOS);
        var maintainerCov = Normalize(maintainer, length, x => x.VANPOS, x => x.TOTPOS);
        var carCov = Normalize(car, length, x => x.VANPOS, x => x.TOTPOS);
        var bikeCov = Normalize(bike, length, x => x.VANPOS, x => x.TOTPOS);
        var pedestrianCov = Normalize(pedestrian, length, x => x.VANPOS, x => x.TOTPOS);

        var ranges = Ranges(length,
            morphologyCov.Pairs(), categoryCov.Pairs(), accessCov.Pairs(), surfaceCov.Pairs(),
            streetNameCov.Pairs(), maintainerCov.Pairs(), carCov.Pairs(), bikeCov.Pairs(), pedestrianCov.Pairs());

        var lengthIndexedLine = new LengthIndexedLine(geometry);
        var result = new List<DerivedRoadSegmentRecord>();

        foreach (var range in ranges)
        {
            var from = range.From;
            var to = range.To;

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
                GEOMETRIE = lengthIndexedLine.ExtractLine(from, range.ToActual).RoundToCm(),
                CREATIE = creatie,
                VERSIE = versie
            });
        }

        return result;
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
