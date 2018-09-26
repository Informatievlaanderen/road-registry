namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class NumberedRoadNumberTests
    {
        private readonly Fixture _fixture;
        private readonly string[] _knownValues;

        public NumberedRoadNumberTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(NumberedRoadNumber.All, type => type.ToString());
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<String>(_fixture),
                new EquatableEqualsSelfAssertion(_fixture),
                new EquatableEqualsOtherAssertion(_fixture),
                new EqualityOperatorEqualsSelfAssertion(_fixture),
                new EqualityOperatorEqualsOtherAssertion(_fixture),
                new InequalityOperatorEqualsSelfAssertion(_fixture),
                new InequalityOperatorEqualsOtherAssertion(_fixture),
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsOtherAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(NumberedRoadNumber));
        }

        [Fact]
        public void AllReturnsExpectedResult()
        {
            Assert.Equal(
                5813,
                NumberedRoadNumber.All.Length);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var sut = NumberedRoadNumber.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            Assert.NotNull(NumberedRoadNumber.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => NumberedRoadNumber.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var result = NumberedRoadNumber.TryParse(value, out NumberedRoadNumber parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToString());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = NumberedRoadNumber.TryParse(value, out NumberedRoadNumber parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
