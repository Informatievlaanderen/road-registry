namespace RoadRegistry.Infrastructure;

using System;
using System.Linq;
using Extensions;
using Gisvl.Gecko.CrsTransformer.NTS;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public static class LambertTransformation
{
    private static class Crs
    {
        public const int Lambert72 = Constants.Crs.Lambert72;
        public const int Lambert08 = Constants.Crs.Lambert2008;
    }

    private static readonly LambertTransformer Lambert72To08Transformer;

    private static readonly OgcGeometryType[] SupportedGeometryTypes =
    [
        OgcGeometryType.Point,
        OgcGeometryType.MultiPoint,
        OgcGeometryType.LineString,
        OgcGeometryType.MultiLineString,
        OgcGeometryType.Polygon,
        OgcGeometryType.MultiPolygon
    ];

    static LambertTransformation()
    {
        Lambert72To08Transformer = new LambertTransformer(CoordinateSystem.Lambert72, CoordinateSystem.Lambert08);
    }

    public static T TransformFromLambert72To08<T>(this T geometry)
        where T : Geometry
    {
        return Lambert72To08Transformer.Transform(geometry, false);
    }
    public static T TransformFromLambert08To72<T>(this T geometry)
        where T : Geometry
    {
        return Lambert72To08Transformer.Transform(geometry, true);
    }

    public static bool IsLambert72(this Geometry geometry)
    {
        return geometry.SRID == CoordinateSystem.Lambert72.GeometryFactory.SRID;
    }
    public static bool IsLambert08(this Geometry geometry)
    {
        return geometry.SRID == CoordinateSystem.Lambert08.GeometryFactory.SRID;
    }

    public static bool IsInsideFlandersUsingLambert72(this Geometry geometry)
    {
        return CoordinateSystem.Lambert72.Contains(geometry);
    }
    public static bool IsInsideFlandersUsingLambert08(this Geometry geometry)
    {
        return CoordinateSystem.Lambert08.Contains(geometry);
    }

    public static T EnsureLambert72<T>(this T geometry)
        where T : Geometry
    {
        return EnsureCoordinatesAreInCoordinateSystem(geometry, () =>
            geometry.IsInsideFlandersUsingLambert08()
                ? geometry.TransformFromLambert08To72()
                : geometry.WithSrid(CoordinateSystem.Lambert72.GeometryFactory.SRID));
    }
    public static T EnsureLambert08<T>(this T geometry)
        where T : Geometry
    {
        return EnsureCoordinatesAreInCoordinateSystem(geometry, () =>
            geometry.IsInsideFlandersUsingLambert72()
                ? geometry.TransformFromLambert72To08()
                : geometry.WithSrid(CoordinateSystem.Lambert08.GeometryFactory.SRID));
    }
    private static T EnsureCoordinatesAreInCoordinateSystem<T>(T geometry, Func<T> transformValidGeometry)
        where T : Geometry
    {
        if (!geometry.IsValid || !SupportedGeometryTypes.Contains(geometry.OgcGeometryType))
        {
            return geometry;
        }

        return transformValidGeometry();
    }

    private sealed class FlandersCoordinateSystem
    {
        public GeometryFactory GeometryFactory { get; }

        private readonly Polygon _boundingBox;

        public FlandersCoordinateSystem(GeometryFactory geometryFactory, string boundingBoxWkt)
        {
            if (geometryFactory.SRID <= 0)
            {
                throw new ArgumentException("SRID must be greater than 0.", nameof(geometryFactory));
            }

            GeometryFactory = geometryFactory;
            _boundingBox = (Polygon)new WKTReader().Read(boundingBoxWkt);
            _boundingBox.SRID = GeometryFactory.SRID;
        }

        public bool Contains(Geometry geometry)
        {
            return geometry.Envelope.Intersects(_boundingBox);
        }
    }

    private static class CoordinateSystem
    {
        public static readonly FlandersCoordinateSystem Lambert72;
        public static readonly FlandersCoordinateSystem Lambert08;

        static CoordinateSystem()
        {
            Lambert72 = new FlandersCoordinateSystem(
                new GeometryFactory(new PrecisionModel(), Crs.Lambert72),
                "POLYGON ((21492.616926517032 152558.5059805829, 21492.616926874573 244521.11300331634, 259366.13819444185 244521.11300324462, 259366.13819474622 152558.50598051306, 21492.616926517032 152558.5059805829))"
            );
            Lambert08 = new FlandersCoordinateSystem(
                new GeometryFactory(new PrecisionModel(), Crs.Lambert08),
                "POLYGON ((521398.8154037502 652516.2224709671, 521398.8154037502 744502.4871143429, 759275.2584176834 744502.4871143429, 759275.2584176834 652516.2224709671, 521398.8154037502 652516.2224709671))"
            );
        }
    }

    private sealed class LambertTransformer
    {
        private readonly FlandersCoordinateSystem _source;
        private readonly FlandersCoordinateSystem _target;
        private readonly FeatureTransformer _transformer;

        public LambertTransformer(FlandersCoordinateSystem source, FlandersCoordinateSystem target)
        {
            _source = source;
            _target = target;
            _transformer = new FeatureTransformer(source.GeometryFactory.SRID, target.GeometryFactory.SRID);
        }

        public T Transform<T>(T geometry, bool inverse) where T : Geometry
        {
            var sourceSrid = inverse ? _target.GeometryFactory.SRID : _source.GeometryFactory.SRID;
            if (geometry.SRID != -1 && geometry.SRID != sourceSrid)
            {
                throw new ArgumentException($"Invalid SRID. Expected {sourceSrid}.", nameof(geometry));
            }

            var targetCoordinateSystem = inverse ? _source : _target;
            var feature = new Feature(geometry, new AttributesTable());
            var transformedGeometry = inverse
                ? _transformer.InverseTransform(feature, targetCoordinateSystem.GeometryFactory).Geometry
                : _transformer.Transform(feature, targetCoordinateSystem.GeometryFactory).Geometry;

            transformedGeometry.SRID = targetCoordinateSystem.GeometryFactory.SRID;

            return (T)transformedGeometry;
        }
    }
}
