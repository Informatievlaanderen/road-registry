namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentDirectionTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentDirectionTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentDirection.All, type => type.ToInt32());
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<int>(_knownValues));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<Int32>(_fixture),
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
            ).Verify(typeof(RoadSegmentDirection));
        }

        [Fact]
        public static void UnknownReturnsExpectedResult()
        {
            Assert.Equal(-8, RoadSegmentDirection.Unknown);
        }

        [Fact]
        public static void ForwardReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentDirection.Forward);
        }

        [Fact]
        public static void BackwardReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentDirection.Backward);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentDirection.Unknown,
                    RoadSegmentDirection.Forward,
                    RoadSegmentDirection.Backward
                },
                RoadSegmentDirection.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentDirection.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentDirection.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentDirection.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentDirection.TryParse(value, out RoadSegmentDirection parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentDirection.TryParse(value, out RoadSegmentDirection parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
