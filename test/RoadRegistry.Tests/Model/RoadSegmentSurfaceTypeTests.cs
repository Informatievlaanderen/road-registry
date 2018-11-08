namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentSurfaceTypeTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentSurfaceTypeTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentSurfaceType.All, type => type.ToInt32());
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
            ).Verify(typeof(RoadSegmentSurfaceType));
        }

        [Fact]
        public static void NotApplicableReturnsExpectedResult()
        {
            Assert.Equal(-9, RoadSegmentSurfaceType.NotApplicable);
        }

        [Fact]
        public static void UnknownReturnsExpectedResult()
        {
            Assert.Equal(-8, RoadSegmentSurfaceType.Unknown);
        }

        [Fact]
        public static void SolidSurfaceReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentSurfaceType.SolidSurface);
        }

        [Fact]
        public static void LooseSurfaceReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentSurfaceType.LooseSurface);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentSurfaceType.NotApplicable,
                    RoadSegmentSurfaceType.Unknown,
                    RoadSegmentSurfaceType.SolidSurface,
                    RoadSegmentSurfaceType.LooseSurface
                },
                RoadSegmentSurfaceType.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentSurfaceType.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentSurfaceType.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentSurfaceType.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentSurfaceType.TryParse(value, out RoadSegmentSurfaceType parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentSurfaceType.TryParse(value, out RoadSegmentSurfaceType parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
