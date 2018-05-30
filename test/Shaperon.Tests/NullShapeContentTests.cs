namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using Wkx;

    public class NullShapeContentTests
    {
        private readonly Fixture _fixture;

        public NullShapeContentTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordNumber();
            _fixture.Register(() => (NullShapeContent)NullShapeContent.Instance);
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void RecordAsReturnsExpectedResult()
        {
            var number = _fixture.Create<RecordNumber>();
            var sut = NullShapeContent.Instance;

            var result = sut.RecordAs(number);

            Assert.Equal(sut, result.Content);
            Assert.Equal(number, result.Header.RecordNumber);
            Assert.Equal(sut.ContentLength, result.Header.ContentLength);
        }

        [Fact]
        public void ReadReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => NullShapeContent.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<NullShapeContent>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void CanReadWriteNullShape()
        {
            var sut = NullShapeContent.Instance;

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
                    var result = (NullShapeContent)NullShapeContent.Read(reader);

                    Assert.Equal(sut.ShapeType, result.ShapeType);
                    Assert.Equal(sut.ContentLength, result.ContentLength);
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
                        () => NullShapeContent.Read(reader)
                    );
                }
            }
        }
    }
}
