namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentLaneDirectionTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentLaneDirectionTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentLaneDirection.All, type => type.ToInt32());
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<int>(_knownValues));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<int>(_fixture),
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
            ).Verify(typeof(RoadSegmentLaneDirection));
        }

        [Fact]
        public static void UnknownReturnsExpectedResult()
        {
            Assert.Equal(-8, RoadSegmentLaneDirection.Unknown);
        }

        [Fact]
        public static void ForwardReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentLaneDirection.Forward);
        }

        [Fact]
        public static void BackwardReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentLaneDirection.Backward);
        }

        [Fact]
        public static void IndependentReturnsExpectedResult()
        {
            Assert.Equal(3, RoadSegmentLaneDirection.Independent);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentLaneDirection.Unknown,
                    RoadSegmentLaneDirection.Forward,
                    RoadSegmentLaneDirection.Backward,
                    RoadSegmentLaneDirection.Independent
                },
                RoadSegmentLaneDirection.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentLaneDirection.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentLaneDirection.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentLaneDirection.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentLaneDirection.TryParse(value, out RoadSegmentLaneDirection parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentLaneDirection.TryParse(value, out RoadSegmentLaneDirection parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
