namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;
    using GeoAPI.Geometries;
    using System.Linq;
    using Infrastucture;
    using KellermanSoftware.CompareNetObjects;

    public class PolyLineMShapeContentTests
    {
        private readonly Fixture _fixture;

        public PolyLineMShapeContentTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<ILineString>(customization =>
                customization.FromFactory(generator =>
                    new LineString(
                        new PointSequence(_fixture.CreateMany<PointM>(generator.Next(2, 10))),
                        GeometryConfiguration.GeometryFactory
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(_fixture.CreateMany<ILineString>(generator.Next(2,20)).ToArray())
                ).OmitAutoProperties()
            );
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReadReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => PolyLineMShapeContent.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<PolyLineMShapeContent>().Select(instance => instance.Write(null)));
        }


        [Fact]
        public void ToBytesHasExpectedResult()
        {
            var sut = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());

            var result = sut.ToBytes();

            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream))
            {
                sut.Write(writer);
                writer.Flush();

                Assert.Equal(stream.ToArray(), result);
            }
        }

        [Fact]
        public void FromBytesHasExpectedResult()
        {
            var content = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());

            var result = PolyLineMShapeContent.FromBytes(content.ToBytes());

            var actual = Assert.IsType<PolyLineMShapeContent>(result);
            Assert.Equal(content.Shape, actual.Shape);
            Assert.Equal(content.ShapeType, actual.ShapeType);
            Assert.Equal(content.ContentLength, actual.ContentLength);
        }

        [Fact]
        public void ToBytesWithEncodingHasExpectedResult()
        {
            var sut = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());

            var result = sut.ToBytes(Encoding.UTF8);

            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                sut.Write(writer);
                writer.Flush();

                Assert.Equal(stream.ToArray(), result);
            }
        }

        [Fact]
        public void FromBytesWithEncodingHasExpectedResult()
        {
            var content = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());

            var result = PolyLineMShapeContent.FromBytes(content.ToBytes(Encoding.UTF8), Encoding.UTF8);

            var actual = Assert.IsType<PolyLineMShapeContent>(result);
            Assert.Equal(content.Shape, actual.Shape);
            Assert.Equal(content.ShapeType, actual.ShapeType);
            Assert.Equal(content.ContentLength, actual.ContentLength);
        }

        [Fact]
        public void CanReadWritePointShape()
        {
            var shape = _fixture.Create<MultiLineString>();
            var sut = new PolyLineMShapeContent(shape);

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = (PolyLineMShapeContent)PolyLineMShapeContent.Read(reader);

                    Assert.Equal(sut.Shape, result.Shape);
                    Assert.Equal(sut.ShapeType, result.ShapeType);
                    Assert.Equal(sut.ContentLength, result.ContentLength);
                }
            }
        }

        [Fact]
        public void CanReadNullShape()
        {
            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    NullShapeContent.Instance.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = PolyLineMShapeContent.Read(reader);

                    Assert.Equal(NullShapeContent.Instance, result);
                }
            }
        }

        [Fact]
        public void ToBytesWithEncodingMatchedDataFromBytesWithEncoding()
        {
            var content = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());
            // check if M values are generated
            Assert.NotEmpty(content
                    .Shape
                    .GetOrdinates(Ordinate.M)
                    .Where(measure => false == double.IsNaN(measure))
            );

            var binaryContent = content.ToBytes(Encoding.UTF8);
            var readAsContent = (PolyLineMShapeContent)PolyLineMShapeContent.FromBytes(binaryContent, Encoding.UTF8);

            Assert.True(content.Shape.EqualsExact(readAsContent.Shape, double.Epsilon));
        }

        [Fact]
        public void ToBytesMatchedDataFromBytes()
        {
            var content = new PolyLineMShapeContent(_fixture.Create<MultiLineString>());
            // check if M values are generated
            Assert.NotEmpty(content
                .Shape
                .GetOrdinates(Ordinate.M)
                .Where(measure => false == double.IsNaN(measure))
            );

            var binaryContent = content.ToBytes();
            var readAsContent = (PolyLineMShapeContent)PolyLineMShapeContent.FromBytes(binaryContent);

            Assert.True(content.Shape.EqualsExact(readAsContent.Shape, double.Epsilon));
        }

        [Theory]
        [InlineData(ShapeType.Point)]
        [InlineData(ShapeType.PolyLine)]
        [InlineData(ShapeType.Polygon)]
        [InlineData(ShapeType.MultiPoint)]
        [InlineData(ShapeType.PointZ)]
        [InlineData(ShapeType.PolyLineZ)]
        [InlineData(ShapeType.PolygonZ)]
        [InlineData(ShapeType.MultiPointZ)]
        [InlineData(ShapeType.PointM)]
        [InlineData(ShapeType.PolygonM)]
        [InlineData(ShapeType.MultiPointM)]
        [InlineData(ShapeType.MultiPatch)]
        public void CanNotReadOtherShapeType(ShapeType other)
        {
            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32LittleEndian((int)other);

                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    Assert.Throws<ShapeRecordContentException>(
                        () => PolyLineMShapeContent.Read(reader)
                    );
                }
            }
        }
    }
}
