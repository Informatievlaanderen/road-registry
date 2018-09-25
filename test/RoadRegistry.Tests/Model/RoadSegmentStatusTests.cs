namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentStatusTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentStatusTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentStatus.All, type => type.ToInt32());
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
            ).Verify(typeof(RoadSegmentStatus));
        }

        [Fact]
        public static void UnknownReturnsExpectedResult()
        {
            Assert.Equal(-8, RoadSegmentStatus.Unknown);
        }

        [Fact]
        public static void PermitRequestedReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentStatus.PermitRequested);
        }

        [Fact]
        public static void BuildingPermitGrantedReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentStatus.BuildingPermitGranted);
        }

        [Fact]
        public static void UnderConstructionReturnsExpectedResult()
        {
            Assert.Equal(3, RoadSegmentStatus.UnderConstruction);
        }

        [Fact]
        public static void InUseReturnsExpectedResult()
        {
            Assert.Equal(4, RoadSegmentStatus.InUse);
        }

        [Fact]
        public static void OutOfUseReturnsExpectedResult()
        {
            Assert.Equal(5, RoadSegmentStatus.OutOfUse);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentStatus.Unknown,
                    RoadSegmentStatus.PermitRequested,
                    RoadSegmentStatus.BuildingPermitGranted,
                    RoadSegmentStatus.UnderConstruction,
                    RoadSegmentStatus.InUse,
                    RoadSegmentStatus.OutOfUse
                },
                RoadSegmentStatus.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentStatus.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentStatus.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentStatus.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentStatus.TryParse(value, out RoadSegmentStatus parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentStatus.TryParse(value, out RoadSegmentStatus parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
