namespace Shaperon
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class DbaseFieldLengthTests
    {
        private readonly Fixture _fixture;

        public DbaseFieldLengthTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldLength();
        }

        [Fact]
        public void MinLengthReturnsExpectedResult()
        {
            var result = DbaseFieldLength.MinLength;

            Assert.Equal(new DbaseFieldLength(0), result);
        }

        [Fact]
        public void MaxLengthReturnsExpectedResult()
        {
            var result = DbaseFieldLength.MaxLength;

            Assert.Equal(new DbaseFieldLength(254), result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ValueCanNotBeNegative(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DbaseFieldLength(value));
        }

        [Fact]
        public void ToInt32ReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseFieldLengthValue();
            var sut = new DbaseFieldLength(value);

            var result = sut.ToInt32();

            Assert.Equal(value, result);
        }

        [Fact]
        public void ImplicitConversionToInt32ReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseFieldLengthValue();
            var sut = new DbaseFieldLength(value);

            int result = sut;

            Assert.Equal(value, result);
        }

        [Fact]
        public void ToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseFieldLengthValue();
            var sut = new DbaseFieldLength(value);

            var result = sut.ToByte();

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ImplicitConversionToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseFieldLengthValue();
            var sut = new DbaseFieldLength(value);

            byte result = sut;

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ToStringReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseFieldLengthValue();
            var sut = new DbaseFieldLength(value);

            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
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
            ).Verify(typeof(DbaseFieldLength));
        }

        [Fact]
        public void IsEquatableToDbaseFieldLength()
        {
            Assert.IsAssignableFrom<IEquatable<DbaseFieldLength>>(_fixture.Create<DbaseFieldLength>());
        }
    }
}
