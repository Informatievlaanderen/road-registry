namespace Shaperon
{
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    // Default point does not support Ordinate.M

    // Point uses a ICoordinateSequence as underlaying type
    // The default ICoordinateSequenceFactory from NetTopologySuite used to create the underlaying ICoordinateSequence is configured for Ordinate.XYZ

    public class MeasuredPoint : Point
    {
        public MeasuredPoint(Coordinate coordinate)
            : base(
                GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(new []{ coordinate }),
                GeometryConfiguration.GeometryFactory
            )
        { }

        public MeasuredPoint(double x, double y, double z, double m)
            : this(new Coordinate(x, y, z))
        {
            ChangeMeasurement(m);
        }

        public MeasuredPoint(double x, double y, double z)
            : this(new Coordinate(x, y, z))
        { }

        public MeasuredPoint(double x, double y)
            : this(new Coordinate(x, y))
        { }

        public MeasuredPoint(ICoordinateSequence coordinatesSequence)
            : this(coordinatesSequence.GetCoordinate(0))
        { }

        // Values cannot be modified, so let's remove the Setters
        public new double X => base.X;
        public new double Y => base.Y;
        public new double Z => base.Z;
        public new double M => base.M;

        public void ChangeMeasurement(double m)
        {
            base.M = m;
        }
    }
}
