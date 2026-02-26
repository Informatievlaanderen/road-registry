namespace RoadRegistry.ValueObjects;

using System;
using NetTopologySuite.Geometries;

public record GeometryObject : IEquatable<GeometryObject>
{
    public int SRID { get; }
    public string WKT { get; }

    public GeometryObject(int srid, string wkt)
    {
        SRID = srid;
        WKT = wkt;
    }
    protected GeometryObject(Geometry geometry)
        : this(geometry.SRID, geometry.AsText())
    {
    }

    public virtual bool Equals(GeometryObject? other)
    {
        return SRID == other?.SRID && WKT == other?.WKT;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SRID, WKT);
    }
}
