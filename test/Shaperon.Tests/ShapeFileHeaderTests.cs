namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;

    public class ShapeFileHeaderTests
    {
        private readonly Fixture _fixture;

        public ShapeFileHeaderTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordNumber();
            _fixture.CustomizeWordLength();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => ShapeFileHeader.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<ShapeFileHeader>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = new ShapeFileHeader(
                _fixture.Create<WordLength>(),
                _fixture.Create<ShapeType>(),
                _fixture.Create<BoundingBox3D>());

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
                    var result = ShapeFileHeader.Read(reader);

                    Assert.Equal(sut.FileLength, result.FileLength);
                    Assert.Equal(sut.ShapeType, result.ShapeType);
                    Assert.Equal(sut.BoundingBox, result.BoundingBox);
                }
            }
        }
    }
}
