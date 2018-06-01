namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;

    public class DbaseFileHeaderTests
    {
        private readonly Fixture _fixture;

        public DbaseFileHeaderTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeWordLength();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => DbaseFileHeader.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseFileHeader>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = _fixture.Create<DbaseFileHeader>();

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
                    var result = DbaseFileHeader.Read(reader);

                    Assert.Equal(sut.LastUpdated, result.LastUpdated);
                    Assert.Equal(sut.RecordCount, result.RecordCount);
                    Assert.Equal(sut.RecordLength, result.RecordLength);
                    Assert.Equal(sut.RecordFields, result.RecordFields, EqualityComparer<DbaseField>.Default);
                }
            }
        }

        [Fact]
        public void ReadExpectsHeaderToStartWithDbase3Format()
        {
            var start = _fixture.Create<Generator<byte>>().Where(_ => _ != DbaseFileHeader.ExpectedDbaseFormat).First();

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.Write(start);

                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    Assert.Throws<DbaseFileHeaderException>(
                        () => DbaseFileHeader.Read(reader)
                    );
                }
            }
        }

        [Fact]
        public void ReadExpectsHeaderToEndWithTerminator()
        {
            var terminator = _fixture.Create<Generator<byte>>().Where(_ => _ != DbaseFileHeader.Terminator).First();

            var sut = _fixture.Create<DbaseFileHeader>();

            using(var outputStream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(outputStream, Encoding.ASCII, true))
                {
                    sut.Write(writer);

                    writer.Flush();
                }

                var buffer = outputStream.ToArray();

                using(var inputStream = new MemoryStream())
                {
                    inputStream.Write(buffer, 0, buffer.Length - 1);
                    inputStream.Write(new [] { terminator }, 0, 1);

                    inputStream.Position = 0;

                    using(var reader = new BinaryReader(inputStream, Encoding.ASCII, true))
                    {
                        Assert.Throws<DbaseFileHeaderException>(
                            () => DbaseFileHeader.Read(reader)
                        );
                    }
                }
            }
        }
    }
}
