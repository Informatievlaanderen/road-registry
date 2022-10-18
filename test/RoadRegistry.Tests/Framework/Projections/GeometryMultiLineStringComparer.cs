namespace RoadRegistry.Tests.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryMultiLineStringComparer : BaseTypeComparer
{
    public GeometryMultiLineStringComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override void CompareType(CompareParms parms)
    {
        var left = (MultiLineString)parms.Object1;
        var right = (MultiLineString)parms.Object2;

        if (!left.EqualsExact(right)) AddDifference(parms);
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1 == typeof(MultiLineString);
    }
}