namespace RoadRegistry.Tests.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryComparer : BaseTypeComparer
{
    public GeometryComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override void CompareType(CompareParms parms)
    {
        var geometry1 = (Geometry)parms.Object1;
        var geometry2 = (Geometry)parms.Object2;

        if (!geometry1.EqualsExact(geometry2))
        {
            AddDifference(parms);
        }
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1.IsAssignableTo(typeof(Geometry));
    }
}
