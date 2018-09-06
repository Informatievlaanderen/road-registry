namespace Shaperon
{
    using AutoFixture;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using Xunit;

    public class WhenCreatingAPointFromAnCoordinateSequence
    {
        private readonly PointM _SUT;
        private readonly Fixture _fixture;
        private readonly ICoordinateSequence _xySequence;

        public WhenCreatingAPointFromAnCoordinateSequence()
        {
            _fixture = new Fixture();

            _xySequence = new DotSpatialAffineCoordinateSequence(1, Ordinates.XYZ);
            _xySequence.SetOrdinate(0, Ordinate.X, _fixture.Create<double>());
            _xySequence.SetOrdinate(0, Ordinate.Y, _fixture.Create<double>());
            _xySequence.SetOrdinate(0, Ordinate.Z, _fixture.Create<double>());

            _SUT = new PointM(_xySequence);
        }

        [Fact]
        public void ThenXContiansTheOrdinateXValue()
        {
            Assert.Equal(_xySequence.GetOrdinate(0, Ordinate.X), _SUT.X);
        }

        [Fact]
        public void ThenYContiansTheOrdinateYValue()
        {
            Assert.Equal(_xySequence.GetOrdinate(0, Ordinate.Y), _SUT.Y);
        }

        [Fact]
        public void ThenZContiansTheOrdinateZValue()
        {
            Assert.Equal(_xySequence.GetOrdinate(0, Ordinate.Z), _SUT.Z);
        }

        [Fact]
        public void ThenMeasureValueCanBeSet()
        {
            var m = _fixture.Create<double>();
            _SUT.ChangeMeasurement(m);
            Assert.Equal(m, _SUT.M);
        }
    }

    public class WhenCreatingAPointFromACoordinate
    {
        private readonly PointM _SUT;
        private readonly Fixture _fixture;
        private readonly Coordinate _coordinate;

        public WhenCreatingAPointFromACoordinate()
        {
            _fixture = new Fixture();

            _coordinate = new Coordinate(
                _fixture.Create<double>(),
                _fixture.Create<double>(),
                _fixture.Create<double>()
                );
            _SUT = new PointM(_coordinate);
        }

        [Fact]
        public void ThenXContiansTheOrdinateXValue()
        {
            Assert.Equal(_coordinate.X, _SUT.X);
        }

        [Fact]
        public void ThenYContiansTheOrdinateYValue()
        {
            Assert.Equal(_coordinate.Y, _SUT.Y);
        }

        [Fact]
        public void ThenZContiansTheOrdinateZValue()
        {
            Assert.Equal(_coordinate.Z, _SUT.Z);
        }

        [Fact]
        public void ThenMeasureValueCanBeSet()
        {
            var m = _fixture.Create<double>();
            _SUT.ChangeMeasurement(m);
            Assert.Equal(m, _SUT.M);
        }
    }

    public class WhenCreatingAPointFromXYValues
    {
        private readonly PointM _SUT;
        private readonly Fixture _fixture;
        private readonly double _x, _y;

        public WhenCreatingAPointFromXYValues()
        {
            _fixture = new Fixture();

            _x = _fixture.Create<double>();
            _y = _fixture.Create<double>();
            _SUT = new PointM(_x, _y);
        }

        [Fact]
        public void ThenXValueIsSet()
        {
            Assert.Equal(_x, _SUT.X);
        }

        [Fact]
        public void ThenYValueIsSet()
        {
            Assert.Equal(_y, _SUT.Y);
        }

        [Fact]
        public void ThenZValueIsNotSet()
        {
            Assert.Equal(double.NaN, _SUT.Z);
        }

        [Fact]
        public void ThenMeasureValueCanBeSet()
        {
            var m = _fixture.Create<double>();
            _SUT.ChangeMeasurement(m);
            Assert.Equal(m, _SUT.M);
        }
    }

    public class WhenCreatingAPointFromXYZValues
    {
        private readonly PointM _SUT;
        private readonly Fixture _fixture;
        private readonly double _x, _y, _z;

        public WhenCreatingAPointFromXYZValues()
        {
            _fixture = new Fixture();

            _x = _fixture.Create<double>();
            _y = _fixture.Create<double>();
            _z = _fixture.Create<double>();
            _SUT = new PointM(_x, _y, _z);
        }

        [Fact]
        public void ThenXValueIsSet()
        {
            Assert.Equal(_x, _SUT.X);
        }

        [Fact]
        public void ThenYValueIsSet()
        {
            Assert.Equal(_y, _SUT.Y);
        }

        [Fact]
        public void ThenZValueIsSet()
        {
            Assert.Equal(_z, _SUT.Z);
        }

        [Fact]
        public void ThenMeasureValueCanBeSet()
        {
            var m = _fixture.Create<double>();
            _SUT.ChangeMeasurement(m);
            Assert.Equal(m, _SUT.M);
        }
    }

    public class WhenCreatingAPointFromXYZMValues
    {
        private readonly PointM _SUT;
        private readonly Fixture _fixture;
        private readonly double _x, _y, _z, _m;

        public WhenCreatingAPointFromXYZMValues()
        {
            _fixture = new Fixture();

            _x = _fixture.Create<double>();
            _y = _fixture.Create<double>();
            _z = _fixture.Create<double>();
            _m = _fixture.Create<double>();
            _SUT = new PointM(_x, _y, _z, _m);
        }

        [Fact]
        public void ThenXValueIsSet()
        {
            Assert.Equal(_x, _SUT.X);
        }

        [Fact]
        public void ThenYValueIsSet()
        {
            Assert.Equal(_y, _SUT.Y);
        }

        [Fact]
        public void ThenZValueIsSet()
        {
            Assert.Equal(_z, _SUT.Z);
        }

        [Fact]
        public void ThenMValueIsSet()
        {
            Assert.Equal(_m, _SUT.M);
        }

        [Fact]
        public void ThenMeasureValueCanBeSet()
        {
            var m = _fixture.Create<double>();
            _SUT.ChangeMeasurement(m);
            Assert.Equal(m, _SUT.M);
        }
    }
}
