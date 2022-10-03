namespace RoadRegistry.Tests.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryPointComparer : BaseTypeComparer
{
    public GeometryPointComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1 == typeof(Point);
    }

    public override void CompareType(CompareParms parms)
    {
        var left = (Point)parms.Object1;
        var right = (Point)parms.Object2;

        if (!left.EqualsExact(right)) AddDifference(parms);
    }
}
