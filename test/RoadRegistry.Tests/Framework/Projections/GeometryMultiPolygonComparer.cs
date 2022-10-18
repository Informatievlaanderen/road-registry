namespace RoadRegistry.Tests.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryMultiPolygonComparer : BaseTypeComparer
{
    public GeometryMultiPolygonComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override void CompareType(CompareParms parms)
    {
        var left = (MultiPolygon)parms.Object1;
        var right = (MultiPolygon)parms.Object2;

        if (!left.EqualsExact(right)) AddDifference(parms);
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1 == typeof(MultiPolygon);
    }
}