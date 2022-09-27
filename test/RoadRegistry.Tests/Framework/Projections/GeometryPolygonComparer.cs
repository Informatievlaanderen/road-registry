namespace RoadRegistry.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryPolygonComparer : BaseTypeComparer
{
    public GeometryPolygonComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1 == typeof(Polygon);
    }

    public override void CompareType(CompareParms parms)
    {
        var left = (Polygon)parms.Object1;
        var right = (Polygon)parms.Object2;

        if (!left.EqualsExact(right)) AddDifference(parms);
    }
}
