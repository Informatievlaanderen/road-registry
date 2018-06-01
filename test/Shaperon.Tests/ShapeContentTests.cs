namespace Shaperon
{
    using AutoFixture;
    using Xunit;
    using System.IO;
    using System.Linq;

    public class ShapeContentTests
    {
        private readonly Fixture _fixture;

        public ShapeContentTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordNumber();
            _fixture.CustomizeWordLength();
        }

        [Fact]
        public void RecordAsReturnsExpectedResult()
        {
            var number = _fixture.Create<RecordNumber>();
            var sut = new ShapeContentUnderTest(
                _fixture.Create<ShapeType>(),
                _fixture.Create<WordLength>(),
                _fixture.CreateMany<byte>().ToArray()
            );

            var result = sut.RecordAs(number);

            Assert.Equal(sut, result.Content);
            Assert.Equal(number, result.Header.RecordNumber);
            Assert.Equal(sut.ContentLength, result.Header.ContentLength);
        }

        [Fact]
        public void ToBytesReturnsExpectedResult()
        {
            var number = _fixture.Create<RecordNumber>();
            var bytes = _fixture.CreateMany<byte>().ToArray();
            var sut = new ShapeContentUnderTest(
                _fixture.Create<ShapeType>(),
                _fixture.Create<WordLength>(),
                bytes
            );

            var result = sut.ToBytes();

            Assert.Equal(bytes, result);
        }

        private class ShapeContentUnderTest : ShapeContent
        {
            public ShapeContentUnderTest(ShapeType typeOfShape, WordLength contentLength, byte[] content)
            {
                ShapeType = typeOfShape;
                ContentLength = contentLength;
                Content = content;
            }

            public byte[] Content { get; }

            public override void Write(BinaryWriter writer)
            {
                foreach(var value in Content)
                {
                    writer.Write(value);
                }
            }
        }
    }
}
