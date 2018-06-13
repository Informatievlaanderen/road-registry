namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using System;
    using NetTopologySuite.Geometries;
    using GeoAPI.Geometries;
    using System.Linq;

    public class PolyLineMShapeContentTests
    {
        private readonly Fixture _fixture;

        public PolyLineMShapeContentTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<Coordinate>(
                customization => customization.FromFactory<int>(
                    value => new Coordinate(new Random(value).Next(), new Random(value).Next())
                ).OmitAutoProperties()
            );
            _fixture.Customize<LineString>(
                customization => customization.FromFactory<int>(
                    value => new LineString(_fixture.CreateMany<Coordinate>(new Random(value).Next(2, 10)).ToArray())
                ).OmitAutoProperties()
            );
            _fixture.Customize<MultiLineString>(
                customization => customization.FromFactory<int>(
                    value => new MultiLineString(_fixture.CreateMany<LineString>(new Random(value).Next(1,10)).ToArray())
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
