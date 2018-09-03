namespace Shaperon
{
    using System;
    using System.Linq;
    using AutoFixture;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class WKBReaderWriterAssumptions
    {
        private readonly Fixture _fixture;

        public WKBReaderWriterAssumptions()
        {
            _fixture = new Fixture();
            _fixture.Customize<Coordinate>(customization =>
                customization
                    .FromFactory<int>(value =>
                        new Coordinate(
                                _fixture.Create<Double>(),
                                _fixture.Create<Double>(),
                                _fixture.Create<Double>()
                        )
                    )
                    .OmitAutoProperties());
            _fixture.Customize<Point>(customization =>
                customization
                    .FromFactory<int>(value =>
                        new Point(
                            GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(
                                new[]
                                {
                                    new Coordinate(
                                        _fixture.Create<Double>(),
                                        _fixture.Create<Double>(),
                                        _fixture.Create<Double>()
                                    )
                                }),
                            GeometryConfiguration.GeometryFactory
                        )
                        {
                            M = _fixture.Create<Double>()
                        }
                    ).OmitAutoProperties());
            _fixture.Customize<LineString>(customization =>
                customization
                    .FromFactory<int>(value =>
                        new LineString(
                            _fixture
                                .CreateMany<Coordinate>(new Random(value).Next(2, 50))
                                .ToArray()
                        )
                    )
                    .OmitAutoProperties());
            _fixture.Customize<MultiLineString>(customization =>
                customization
                    .FromFactory<int>(value => new MultiLineString(
                        _fixture.CreateMany<LineString>(new Random(value).Next(0, 100)).ToArray(),
                        GeometryConfiguration.GeometryFactory
                        )
                    ).OmitAutoProperties());
        }

        [Fact]
        public void PointAsExtendedWellKnownBinaryCanBeWrittenAndRead()
        {
            var sut = _fixture.Create<Point>();

            var writer =  new WellKnownBinaryWriter();

            var extendedWellKnownBinary = writer.Write(sut);

            var reader = new WellKnownBinaryReader();

            var result = reader.Read(extendedWellKnownBinary);

            Assert.NotNull(result);
            Assert.Equal(sut, Assert.IsType<Point>(result));
            Assert.Equal(sut.SRID, ((Point)result).SRID);
            Assert.Equal(sut.M, ((Point)result).M);
        }

        [Fact]
        public void MultiLineStringAsExtendedWellKnownBinaryCanBeWrittenAndRead()
        {
            var sut = _fixture.Create<MultiLineString>();

            var writer = new WellKnownBinaryWriter();

            var extendedWellKnownBinary = writer.Write(sut);

            var reader = new WellKnownBinaryReader();

            var result = reader.Read(extendedWellKnownBinary);

            Assert.NotNull(result);
            Assert.Equal(sut, Assert.IsType<MultiLineString>(result));
            Assert.Equal(sut.SRID, ((MultiLineString)result).SRID);
        }
    }
}
