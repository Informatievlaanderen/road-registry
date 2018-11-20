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
                                point => new PointM(point.X, point.M, 0.0, point.M)
                                {
                                    SRID = geometry.SpatialReferenceSystemIdentifier
                                })),
                        GeometryConfiguration.GeometryFactory)

                    {
                        SRID = geometry.SpatialReferenceSystemIdentifier
                    }));
        }

        public static Messages.RoadSegmentGeometry Translate(NetTopologySuite.Geometries.MultiLineString geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            var multiLineString = Array.ConvertAll(
                geometry.Geometries.Cast<NetTopologySuite.Geometries.LineString>().ToArray(),
                input => new Messages.LineString
                {
                    Points = Array.ConvertAll(
                        input.Coordinates,
                        coordinate => new Messages.PointWithM
                        {
                            X = coordinate.X,
                            Y = coordinate.Y
                        })
                });
            var points = multiLineString.SelectMany(line => line.Points).ToArray();
            var measures = geometry.GetOrdinates(Ordinate.M);
            for (var index = 0; index < points.Length && index < measures.Length; index++)
            {
                points[index].M = measures[index];
            }
            return new Messages.RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                MultiLineString = multiLineString
            };
        }
    }
}
