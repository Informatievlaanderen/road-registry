namespace Shaperon
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class WordOffsetTests
    {
        private readonly Fixture _fixture;

        public WordOffsetTests()
        {
            _fixture = new Fixture();
            _fixture.Customizations.Add(new Int32SequenceGenerator());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ValueCanNotBeNegative(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WordOffset(value));
        }

        [Fact]
        public void ToInt32ReturnsExpectedValue()
        {
            var value = _fixture.Create<int>();
            var sut = new WordOffset(value);

            var result = sut.ToInt32();

            Assert.Equal(value, result);
        }

        [Fact]
        public void ToByteLengthReturnsExpectedValue()
        {
            var value = _fixture.Create<int>();
            var sut = new WordOffset(value);

            var result = sut.ToByteLength();

            Assert.Equal(new ByteLength(value * 2), result);
        }
    }
}