namespace RoadRegistry.Framework.Projections;

using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using NetTopologySuite.Geometries;

public class GeometryLineStringComparer : BaseTypeComparer
{
    public GeometryLineStringComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override bool IsTypeMatch(Type type1, Type type2)
    {
        return type1 == typeof(LineString);
    }

    public override void CompareType(CompareParms parms)
    {
        var lineString1 = (LineString)parms.Object1;
        var lineString2 = (LineString)parms.Object2;

        if (!lineString1.EqualsExact(lineString2)) AddDifference(parms);
    }
}
