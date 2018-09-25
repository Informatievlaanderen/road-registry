namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentAccessRestrictionTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentAccessRestrictionTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentAccessRestriction.All, type => type.ToInt32());
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
            ).Verify(typeof(RoadSegmentAccessRestriction));
        }

        [Fact]
        public static void PublicRoadReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentAccessRestriction.PublicRoad);
        }

        [Fact]
        public static void PhysicallyImpossibleReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentAccessRestriction.PhysicallyImpossible);
        }

        [Fact]
        public static void LegallyForbiddenReturnsExpectedResult()
        {
            Assert.Equal(3, RoadSegmentAccessRestriction.LegallyForbidden);
        }

        [Fact]
        public static void PrivateRoadReturnsExpectedResult()
        {
            Assert.Equal(4, RoadSegmentAccessRestriction.PrivateRoad);
        }

        [Fact]
        public static void SeasonalReturnsExpectedResult()
        {
            Assert.Equal(5, RoadSegmentAccessRestriction.Seasonal);
        }

        [Fact]
        public static void TollReturnsExpectedResult()
        {
            Assert.Equal(6, RoadSegmentAccessRestriction.Toll);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentAccessRestriction.PublicRoad,
                    RoadSegmentAccessRestriction.PhysicallyImpossible,
                    RoadSegmentAccessRestriction.LegallyForbidden,
                    RoadSegmentAccessRestriction.PrivateRoad,
                    RoadSegmentAccessRestriction.Seasonal,
                    RoadSegmentAccessRestriction.Toll
                },
                RoadSegmentAccessRestriction.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentAccessRestriction.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentAccessRestriction.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentAccessRestriction.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentAccessRestriction.TryParse(value, out RoadSegmentAccessRestriction parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentAccessRestriction.TryParse(value, out RoadSegmentAccessRestriction parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
