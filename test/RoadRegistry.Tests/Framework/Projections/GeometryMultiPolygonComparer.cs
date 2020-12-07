namespace RoadRegistry.Framework.Projections
{
    using System;
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using NetTopologySuite.Geometries;

    public class GeometryMultiPolygonComparer : BaseTypeComparer
    {
        public GeometryMultiPolygonComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == typeof (MultiPolygon);
        }

        public override void CompareType(CompareParms parms)
        {
            var multiPolygon1 = (MultiPolygon)parms.Object1;
            var multiPolygon2 = (MultiPolygon)parms.Object2;

            if (!multiPolygon1.EqualsExact(multiPolygon2))
            {
                AddDifference(parms);
            }
        }
    }
}
