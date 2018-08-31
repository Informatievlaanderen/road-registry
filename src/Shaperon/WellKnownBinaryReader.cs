namespace Shaperon
{
    using GeoAPI.Geometries;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;

    public class WellKnownBinaryReader
    {
        private readonly WKBReader _wkbReader;

        private static readonly SpatialReferenceSystemIdentifier DefaultWkbSpatialReferenceSystemIdentifier = new SpatialReferenceSystemIdentifier(-1);

        public WellKnownBinaryReader()
            : this(DefaultWkbSpatialReferenceSystemIdentifier)
        { }

        public WellKnownBinaryReader(SpatialReferenceSystemIdentifier spatialReferenceSystemIdentifier)
        {
            var services = new NtsGeometryServices(
                new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZM),
                new PrecisionModel(PrecisionModels.Floating),
                spatialReferenceSystemIdentifier);

            _wkbReader = new WKBReader(services)
            {
                HandleOrdinates = Ordinates.XYZM,
                HandleSRID = true
            };
        }

        public IGeometry Read(byte[] data)
        {
            return _wkbReader.Read(data);
        }

        public TGeometry ReadAs<TGeometry>(byte[] value)
            where TGeometry : IGeometry
        {
            return (TGeometry)_wkbReader.Read(value);
        }

        public bool TryReadAs<TGeometry>(byte[] value, out TGeometry geometry)
            where TGeometry : IGeometry
        {
            var parsed = _wkbReader.Read(value);
            if (parsed is TGeometry parsedGeometry)
            {
                geometry = parsedGeometry;
                return true;
            }
            geometry = default(TGeometry);
            return false;
        }
    }
}
