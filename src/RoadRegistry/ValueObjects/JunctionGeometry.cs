namespace RoadRegistry.ValueObjects;

using System;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

// The point geometry of a junction (grade or grade-separated kruising): the intersection of the two linked road segments,
// rounded to cm. Shared by both junction aggregates. Mirrors RoadNodeGeometry (a single Point).
public sealed record JunctionGeometry : GeometryObject, IEquatable<JunctionGeometry>
{
    private Point? _geometry;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public Point Value
    {
        get
        {
            _geometry ??= (Point)new WKTReader().Read(WKT)
                .WithSrid(SRID);
            return _geometry;
        }
    }

    public JunctionGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private JunctionGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static JunctionGeometry Create(Point geometry)
    {
        return new JunctionGeometry(geometry);
    }

    public bool Equals(JunctionGeometry? other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
