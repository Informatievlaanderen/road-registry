namespace Shaperon
{
    using System;
    using System.IO;
    using System.Text;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class ShapeIndexRecordTests
    {
        private readonly Fixture _fixture;

        public ShapeIndexRecordTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeWordLength();
            _fixture.CustomizeWordOffset();
            _fixture.CustomizeShapeRecordCount();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => ShapeIndexRecord.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<ShapeIndexRecord>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void GetTotalLengthForRecordCountReturnsExpectedResult()
        {
            var count = _fixture.Create<ShapeRecordCount>();

            var result = ShapeIndexRecord.GetTotalLengthForRecordCount(count);

            Assert.Equal(new WordLength(count * 4), result);
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = new ShapeIndexRecord(
                _fixture.Create<WordOffset>(),
                _fixture.Create<WordLength>());

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
                    var result = ShapeIndexRecord.Read(reader);

                    Assert.Equal(sut.Offset, result.Offset);
                    Assert.Equal(sut.ContentLength, result.ContentLength);
                }
            }
        }
    }
}
