namespace Shaperon
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class DbaseDecimalCountTests
    {
        private readonly Fixture _fixture;

        public DbaseDecimalCountTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseDecimalCount();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ValueCanNotBeNegative(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DbaseDecimalCount(value));
        }

        [Fact]
        public void ToInt32ReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseDecimalCountValue();
            var sut = new DbaseDecimalCount(value);

            var result = sut.ToInt32();

            Assert.Equal(value, result);
        }

        [Fact]
        public void ImplicitConversionToInt32ReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseDecimalCountValue();
            var sut = new DbaseDecimalCount(value);

            int result = sut;

            Assert.Equal(value, result);
        }

        [Fact]
        public void ToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseDecimalCountValue();
            var sut = new DbaseDecimalCount(value);

            var result = sut.ToByte();

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ImplicitConversionToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseDecimalCountValue();
            var sut = new DbaseDecimalCount(value);

            byte result = sut;

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ToStringReturnsExpectedValue()
        {
            var value = _fixture.Create<int>().AsDbaseDecimalCountValue();
            var sut = new DbaseDecimalCount(value);

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
            ).Verify(typeof(DbaseDecimalCount));
        }

        [Fact]
        public void IsEquatableToDbaseDecimalCount()
        {
            Assert.IsAssignableFrom<IEquatable<DbaseDecimalCount>>(_fixture.Create<DbaseDecimalCount>());
        }
    }
}
