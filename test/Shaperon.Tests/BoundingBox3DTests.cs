namespace Shaperon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using GeoAPI.Geometries;
    using Infrastucture;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class BoundingBox3DTests
    {
        private readonly Fixture _fixture;

        public BoundingBox3DTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyEquality()
        {
            new CompositeIdiomaticAssertion(
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(BoundingBox3D));
        }

        [Theory]
        [MemberData(nameof(ExpandWithCases))]
        public void ExpandWithReturnsExpectedResult(BoundingBox3D sut, BoundingBox3D other, BoundingBox3D expected)
        {
            var result = sut.ExpandWith(other);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> ExpandWithCases
        {
            get
            {
                var fixture = new Fixture();
                var generator = new Generator<double>(fixture);
                var sut = new BoundingBox3D(
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>()
                    );
                var bigger = new BoundingBox3D(
                        sut.XMin - 1,
                        sut.YMin - 1,
                        sut.XMax + 1,
                        sut.YMax + 1,
                        sut.ZMin - 1,
                        sut.ZMax + 1,
                        sut.MMin - 1,
                        sut.MMax + 1
                    );
                var smaller = new BoundingBox3D(
                        sut.XMin + 1,
                        sut.YMin + 1,
                        sut.XMax - 1,
                        sut.YMax - 1,
                        sut.ZMin + 1,
                        sut.ZMax - 1,
                        sut.MMin + 1,
                        sut.MMax - 1
                    );

                yield return new object[]
                {
                    sut,
                    bigger,
                    bigger
                };

                yield return new object[]
                {
                    sut,
                    smaller,
                    sut
                };

                // more cases we can think of but this is a good start
            }
        }

        [Theory]
        [MemberData(nameof(FromGeometryCases))]
        public void FromGeometryReturnsExpectedResult(IGeometry geometry, BoundingBox3D expected)
        {
            var result = BoundingBox3D.FromGeometry(geometry);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> FromGeometryCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.Customize<MeasuredPoint>(customization =>
				    customization.FromFactory(generator =>
					     new MeasuredPoint(
                            fixture.Create<double>(),
                            fixture.Create<double>(),
                            fixture.Create<double>(),
                            fixture.Create<double>()
                        )
                    )
                );

                fixture.Customize<ILineString>(customization =>
				    customization.FromFactory(generator =>
					    new LineString(
                            new PointSequence(fixture.CreateMany<MeasuredPoint>()),
                            GeometryConfiguration.GeometryFactory)
                    ).OmitAutoProperties()
                );
                fixture.Customize<MultiLineString>(customization =>
                    customization.FromFactory(generator =>
                        new MultiLineString(fixture.CreateMany<ILineString>(generator.Next(1,10)).ToArray())
                    ).OmitAutoProperties()
                );
                var point1 = new MeasuredPoint(
                    fixture.Create<double>(),
                    fixture.Create<double>(),
                    fixture.Create<double>(),
                    fixture.Create<double>()
                );

                yield return new object[]
                {
                    point1,
                    new BoundingBox3D(
                        point1.X,
                        point1.Y,
                        point1.X,
                        point1.Y,
                        point1.Z,
                        point1.Z,
                        point1.M,
                        point1.M
                    )
                };

                var point2 = new MeasuredPoint(
                    fixture.Create<double>(),
                    fixture.Create<double>(),
                    double.NaN,
                    double.NaN
                );

                yield return new object[]
                {
                    point2,
                    new BoundingBox3D(
                        point2.X,
                        point2.Y,
                        point2.X,
                        point2.Y,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN
                    )
                };


                var linestring1 = fixture.Create<MultiLineString>();

                yield return new object[]
                {
                    linestring1,
                    new BoundingBox3D(
                        linestring1.GetOrdinates(Ordinate.X).Min(),
                        linestring1.GetOrdinates(Ordinate.Y).Min(),
                        linestring1.GetOrdinates(Ordinate.X).Max(),
                        linestring1.GetOrdinates(Ordinate.Y).Max(),
                        linestring1.GetOrdinates(Ordinate.Z).DefaultIfEmpty(Double.NaN).Min(),
                        linestring1.GetOrdinates(Ordinate.Z).DefaultIfEmpty(Double.NaN).Max(),
                        linestring1.GetOrdinates(Ordinate.M).DefaultIfEmpty(Double.NaN).Min(),
                        linestring1.GetOrdinates(Ordinate.M).DefaultIfEmpty(Double.NaN).Max()
                    )
                };

                fixture.Customize<ILineString>(customization =>
                    customization.FromFactory(generator =>
                        new LineString(
                            new PointSequence(
                                fixture
                                    .CreateMany<MeasuredPoint>(generator.Next(2, 10))
                                    .Select(point => new MeasuredPoint(point.X, point.Y, double.NaN, double.NaN))
                                ),
                             GeometryConfiguration.GeometryFactory
                        )
                    ).OmitAutoProperties()
                );

                var linestring2 = fixture.Create<MultiLineString>();

                yield return new object[]
                {
                    linestring2,
                    new BoundingBox3D(
                        linestring2.GetOrdinates(Ordinate.X).Min(),
                        linestring2.GetOrdinates(Ordinate.Y).Min(),
                        linestring2.GetOrdinates(Ordinate.X).Max(),
                        linestring2.GetOrdinates(Ordinate.Y).Max(),
                        Double.NaN,
                        Double.NaN,
                        Double.NaN,
                        Double.NaN
                    )
                };
            }
        }
    }
}
