namespace RoadRegistry.Pbs.Projections;

using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.RoadSegment.Flattening;
using RoadRegistry.RoadSegment.ValueObjects;
using Schema.Records;
using static RoadRegistry.RoadSegment.Flattening.RoadSegmentFlattenEngine;

// Flattens a road segment into AfgeleideWegsegmenten rows: the segment is split at every position where any dynamic
// attribute changes, and for each sub-range the attributes are resolved to plain values and a sub-geometry is cut from
// the segment geometry. Works off the per-attribute Att records (which carry VANPOS/TOTPOS/code/label/KANT). The
// split/normalize/resolve algorithm is shared with the extract and WmsWfsV2 flatteners via RoadSegmentFlattenEngine.
internal static class DerivedWegsegmentFlattener
{
    private static int SideLeft => RoadSegmentAttributeSide.Links.Translation.Identifier;
    private static int SideRight => RoadSegmentAttributeSide.Rechts.Translation.Identifier;

    public static List<DerivedRoadSegmentRecord> Flatten(
        int segId,
        Geometry geometry,
        int? status, string lblStatus, int? method, string lblMethod, int? beginNodeId, int? endNodeId,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord> morphology,
        IReadOnlyList<RoadSegmentCategoryAttributeRecord> category,
        IReadOnlyList<RoadSegmentAccessRestrictionAttributeRecord> access,
        IReadOnlyList<RoadSegmentSurfaceTypeAttributeRecord> surface,
        IReadOnlyList<RoadSegmentStreetNameAttributeRecord> streetName,
        IReadOnlyList<RoadSegmentMaintenanceAuthorityAttributeRecord> maintainer,
        IReadOnlyList<RoadSegmentCarTrafficDirectionAttributeRecord> car,
        IReadOnlyList<RoadSegmentBikeTrafficDirectionAttributeRecord> bike,
        IReadOnlyList<RoadSegmentPedestrianTrafficDirectionAttributeRecord> pedestrian,
        string creatie, string versie)
    {
        // The caller hands over the segment geometry as it is held on the record, which - once the record has been read
        // back from SQL Server - carries a declared Z and M ordinate: SqlServerBytesReader always builds coordinate
        // sequences with both, NaN-valued, even for a column holding plain 2D geometries. Everything cut out of the
        // geometry below inherits that, and would be written back to the target database as a 3D/measured geometry.
        // Normalizing once here keeps every derived row 2D, whichever event re-derived the segment.
        geometry = geometry.Force2D();

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

            // Rounding the cut sub-line to centimetres can collapse an interpolated cut point onto the vertex next to
            // it; the repeated coordinate would make the geometry invalid for SQL Server. A sub-range whose geometry
            // has no length left at all is not worth a row.
            var geometrie = ((LineString)lengthIndexedLine.ExtractLine(from, range.ToActual)).RoundToCm().WithoutRepeatedCoordinates();
            if (geometrie is null)
            {
                continue;
            }

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
                AUTOHEEN = Heen(carR?.RICHTING),
                AUTOTERUG = Terug(carR?.RICHTING),
                FIETSHEEN = Heen(bikeR?.RICHTING),
                FIETSTERUG = Terug(bikeR?.RICHTING),
                VOETGANGER = Voetganger(pedR?.RICHTING),
                GEOMETRIE = geometrie,
                CREATIE = creatie,
                VERSIE = versie
            });
        }

        return result;
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
