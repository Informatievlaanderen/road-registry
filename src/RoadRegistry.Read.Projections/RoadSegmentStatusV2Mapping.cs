namespace RoadRegistry.Read.Projections;

using System;
using RoadRegistry.ValueObjects;

// The status and geometry draw method a road segment has in the V2 road network. A V2 road segment carries them in its
// read document; a V1 road segment - one not migrated yet - carries its V1 values, which map onto V2 here.
//
// The detail of a road segment shows these, and the list of road segments filters on the status, through the status
// column 20261113120000_add_status_column_to_read_roadsegments generates from the same document. That column repeats
// this mapping in SQL; RoadSegmentStatusV2MappingTests in the integration tests holds the two side by side.
public static class RoadSegmentStatusV2Mapping
{
    public static RoadSegmentStatusV2 GetStatusV2(this RoadSegmentReadItem roadSegment)
    {
        return roadSegment.IsV2
            ? RoadSegmentStatusV2.Parse(roadSegment.Status)
            : MapToV2(RoadSegmentStatus.Parse(roadSegment.Status), roadSegment.GetGeometryDrawMethodV2());
    }

    public static RoadSegmentGeometryDrawMethodV2 GetGeometryDrawMethodV2(this RoadSegmentReadItem roadSegment)
    {
        return roadSegment.IsV2
            ? RoadSegmentGeometryDrawMethodV2.Parse(roadSegment.GeometryDrawMethod)
            : MapToV2(RoadSegmentGeometryDrawMethod.Parse(roadSegment.GeometryDrawMethod));
    }

    public static RoadSegmentStatusV2 MapToV2(RoadSegmentStatus status, RoadSegmentGeometryDrawMethodV2 method)
    {
        if (status == RoadSegmentStatus.PermitRequested
            || status == RoadSegmentStatus.PermitGranted
            || status == RoadSegmentStatus.UnderConstruction)
        {
            return RoadSegmentStatusV2.Gepland;
        }

        if (status == RoadSegmentStatus.InUse)
        {
            return RoadSegmentStatusV2.Gerealiseerd;
        }

        if (status == RoadSegmentStatus.OutOfUse)
        {
            return RoadSegmentStatusV2.BuitenGebruik;
        }

        if (status == RoadSegmentStatus.Unknown)
        {
            return method == RoadSegmentGeometryDrawMethodV2.Ingemeten
                ? RoadSegmentStatusV2.Gerealiseerd
                : RoadSegmentStatusV2.Gepland;
        }

        throw new InvalidOperationException($"Unknown status value '{status}'");
    }

    public static RoadSegmentGeometryDrawMethodV2 MapToV2(RoadSegmentGeometryDrawMethod method)
    {
        return method.ToString() switch
        {
            nameof(RoadSegmentGeometryDrawMethod.Outlined) => RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            nameof(RoadSegmentGeometryDrawMethod.Measured) => RoadSegmentGeometryDrawMethodV2.Ingemeten,
            nameof(RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications) => RoadSegmentGeometryDrawMethodV2.Ingemeten,
            _ => throw new InvalidOperationException($"Unknown geometry draw method value '{method}'")
        };
    }
}
