namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class EuropeanRoadNumberTests
    {
        private readonly Fixture _fixture;
        private readonly string[] _knownValues;

        public EuropeanRoadNumberTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(EuropeanRoadNumber.All, type => type.ToString());
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
            ).Verify(typeof(EuropeanRoadNumber));
        }

        [Fact]
        public void E17ReturnsExpectedResult()
        {
            Assert.Equal("E17", EuropeanRoadNumber.E17);
        }

        [Fact]
        public void E19ReturnsExpectedResult()
        {
            Assert.Equal("E19", EuropeanRoadNumber.E19);
        }

        [Fact]
        public void E25ReturnsExpectedResult()
        {
            Assert.Equal("E25", EuropeanRoadNumber.E25);
        }

        [Fact]
        public void E313ReturnsExpectedResult()
        {
            Assert.Equal("E313", EuropeanRoadNumber.E313);
        }

        [Fact]
        public void E314ReturnsExpectedResult()
        {
            Assert.Equal("E314", EuropeanRoadNumber.E314);
        }

        [Fact]
        public void E34ReturnsExpectedResult()
        {
            Assert.Equal("E34", EuropeanRoadNumber.E34);
        }

        [Fact]
        public void E40ReturnsExpectedResult()
        {
            Assert.Equal("E40", EuropeanRoadNumber.E40);
        }

        [Fact]
        public void E403ReturnsExpectedResult()
        {
            Assert.Equal("E403", EuropeanRoadNumber.E403);
        }

        [Fact]
        public void E411ReturnsExpectedResult()
        {
            Assert.Equal("E411", EuropeanRoadNumber.E411);
        }

        [Fact]
        public void E429ReturnsExpectedResult()
        {
            Assert.Equal("E429", EuropeanRoadNumber.E429);
        }

        [Fact]
        public void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    EuropeanRoadNumber.E17,
                    EuropeanRoadNumber.E19,
                    EuropeanRoadNumber.E25,
                    EuropeanRoadNumber.E313,
                    EuropeanRoadNumber.E314,
                    EuropeanRoadNumber.E34,
                    EuropeanRoadNumber.E40,
                    EuropeanRoadNumber.E403,
                    EuropeanRoadNumber.E411,
                    EuropeanRoadNumber.E429
                },
                EuropeanRoadNumber.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var sut = EuropeanRoadNumber.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            Assert.NotNull(EuropeanRoadNumber.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => EuropeanRoadNumber.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var result = EuropeanRoadNumber.TryParse(value, out EuropeanRoadNumber parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToString());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = EuropeanRoadNumber.TryParse(value, out EuropeanRoadNumber parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
