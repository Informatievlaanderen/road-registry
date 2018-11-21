namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;

    public static class GeometryTranslator
    {
        public static NetTopologySuite.Geometries.Point Translate(Messages.RoadNodeGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new NetTopologySuite.Geometries.Point(geometry.Point.X, geometry.Point.Y)
            {
                SRID = geometry.SpatialReferenceSystemIdentifier
            };
        }

        public static Messages.RoadNodeGeometry Translate(NetTopologySuite.Geometries.Point geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new Messages.RoadNodeGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                Point = new Messages.Point
                {
                    X = geometry.X,
                    Y = geometry.Y
                }
            };
        }

        public static NetTopologySuite.Geometries.MultiLineString Translate(Messages.RoadSegmentGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new NetTopologySuite.Geometries.MultiLineString(
                Array.ConvertAll(
                    geometry.MultiLineString,
                    line => (ILineString) new NetTopologySuite.Geometries.LineString(
                        new PointSequence(
                            Array.ConvertAll(
                                line.Points,
                                point => new PointM(point.X, point.Y, double.NaN, point.M)
                                {
                                    SRID = geometry.SpatialReferenceSystemIdentifier
                                })),
                        GeometryConfiguration.GeometryFactory)

                    {
                        SRID = geometry.SpatialReferenceSystemIdentifier
                    }),
                GeometryConfiguration.GeometryFactory);
        }

        public static Messages.RoadSegmentGeometry Translate(NetTopologySuite.Geometries.MultiLineString geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            var multiLineString = new Messages.LineString[geometry.NumGeometries];
            var lineIndex = 0;
            foreach (var fromLineString in geometry.Geometries.OfType<NetTopologySuite.Geometries.LineString>())
            {
                var toLineString = new Messages.LineString
                {
                    Points = new Messages.PointWithM[fromLineString.NumPoints]
                };

                for (var pointIndex = 0; pointIndex < fromLineString.NumPoints; pointIndex++)
                {
                    toLineString.Points[pointIndex] = new Messages.PointWithM
                    {
                        X = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.X),
                        Y = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.Y),
                        M = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.M)
                    };
                }

                multiLineString[lineIndex] = toLineString;
                lineIndex++;
            }
            return new Messages.RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                MultiLineString = multiLineString
            };
        }
    }
}
