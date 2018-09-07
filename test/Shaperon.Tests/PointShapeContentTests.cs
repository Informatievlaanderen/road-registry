namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using Infrastucture;

    public class PointShapeContentTests
    {
        private readonly Fixture _fixture;

        public PointShapeContentTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(_fixture.Create<double>(), _fixture.Create<double>())
                ).OmitAutoProperties()
            );
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReadReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => PointShapeContent.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<PointShapeContent>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void ToBytesHasExpectedResult()
        {
            var sut = new PointShapeContent(_fixture.Create<PointM>());

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
            var content = new PointShapeContent(_fixture.Create<PointM>());

            var result = PointShapeContent.FromBytes(content.ToBytes());

            var actual = Assert.IsType<PointShapeContent>(result);
            Assert.Equal(content.Shape, actual.Shape);
            Assert.Equal(content.ShapeType, actual.ShapeType);
            Assert.Equal(content.ContentLength, actual.ContentLength);
        }

        [Fact]
        public void ToBytesWithEncodingHasExpectedResult()
        {
            var sut = new PointShapeContent(_fixture.Create<PointM>());

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
            var content = new PointShapeContent(_fixture.Create<PointM>());

            var result = PointShapeContent.FromBytes(content.ToBytes(Encoding.UTF8), Encoding.UTF8);

            var actual = Assert.IsType<PointShapeContent>(result);
            Assert.Equal(content.Shape, actual.Shape);
            Assert.Equal(content.ShapeType, actual.ShapeType);
            Assert.Equal(content.ContentLength, actual.ContentLength);
        }

        [Fact]
        public void CanReadWritePointShape()
        {
            var sut = new PointShapeContent(_fixture.Create<PointM>());

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
                    var result = (PointShapeContent)PointShapeContent.Read(reader);

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
                    var result = PointShapeContent.Read(reader);

                    Assert.Equal(NullShapeContent.Instance, result);
                }
            }
        }

        [Theory]
        [InlineData(ShapeType.PolyLine)]
        [InlineData(ShapeType.Polygon)]
        [InlineData(ShapeType.MultiPoint)]
        [InlineData(ShapeType.PointZ)]
        [InlineData(ShapeType.PolyLineZ)]
        [InlineData(ShapeType.PolygonZ)]
        [InlineData(ShapeType.MultiPointZ)]
        [InlineData(ShapeType.PointM)]
        [InlineData(ShapeType.PolyLineM)]
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
                        () => PointShapeContent.Read(reader)
                    );
                }
            }
        }
    }
}
