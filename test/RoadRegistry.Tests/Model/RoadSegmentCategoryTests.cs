namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentCategoryTests
    {
        private readonly Fixture _fixture;
        private readonly string[] _knownValues;

        public RoadSegmentCategoryTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentCategory.All, type => type.ToString());
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
            ).Verify(typeof(RoadSegmentCategory));
        }

        [Fact]
        public void UnknownReturnsExpectedResult()
        {
            Assert.Equal("-8", RoadSegmentCategory.Unknown);
        }

        [Fact]
        public void NotApplicableReturnsExpectedResult()
        {
            Assert.Equal("-9", RoadSegmentCategory.NotApplicable);
        }

        [Fact]
        public void MainRoadReturnsExpectedResult()
        {
            Assert.Equal("H", RoadSegmentCategory.MainRoad);
        }

        [Fact]
        public void LocalRoadReturnsExpectedResult()
        {
            Assert.Equal("L", RoadSegmentCategory.LocalRoad);
        }

        [Fact]
        public void LocalRoadType1ReturnsExpectedResult()
        {
            Assert.Equal("L1", RoadSegmentCategory.LocalRoadType1);
        }

        [Fact]
        public void LocalRoadType2ReturnsExpectedResult()
        {
            Assert.Equal("L2", RoadSegmentCategory.LocalRoadType2);
        }

        [Fact]
        public void LocalRoadType3ReturnsExpectedResult()
        {
            Assert.Equal("L3", RoadSegmentCategory.LocalRoadType3);
        }

        [Fact]
        public void PrimaryRoadIReturnsExpectedResult()
        {
            Assert.Equal("PI", RoadSegmentCategory.PrimaryRoadI);
        }

        [Fact]
        public void PrimaryRoadIIReturnsExpectedResult()
        {
            Assert.Equal("PII", RoadSegmentCategory.PrimaryRoadII);
        }

        [Fact]
        public void PrimaryRoadIIType1ReturnsExpectedResult()
        {
            Assert.Equal("PII-1", RoadSegmentCategory.PrimaryRoadIIType1);
        }

        [Fact]
        public void PrimaryRoadIIType2ReturnsExpectedResult()
        {
            Assert.Equal("PII-2", RoadSegmentCategory.PrimaryRoadIIType2);
        }

        [Fact]
        public void PrimaryRoadIIType3ReturnsExpectedResult()
        {
            Assert.Equal("PII-3", RoadSegmentCategory.PrimaryRoadIIType3);
        }

        [Fact]
        public void PrimaryRoadIIType4ReturnsExpectedResult()
        {
            Assert.Equal("PII-4", RoadSegmentCategory.PrimaryRoadIIType4);
        }

        [Fact]
        public void SecondaryRoadReturnsExpectedResult()
        {
            Assert.Equal("S", RoadSegmentCategory.SecondaryRoad);
        }

        [Fact]
        public void SecondaryRoadType1ReturnsExpectedResult()
        {
            Assert.Equal("S1", RoadSegmentCategory.SecondaryRoadType1);
        }

        [Fact]
        public void SecondaryRoadType2ReturnsExpectedResult()
        {
            Assert.Equal("S2", RoadSegmentCategory.SecondaryRoadType2);
        }

         [Fact]
        public void SecondaryRoadType3ReturnsExpectedResult()
        {
            Assert.Equal("S3", RoadSegmentCategory.SecondaryRoadType3);
        }

         [Fact]
        public void SecondaryRoadType4ReturnsExpectedResult()
        {
            Assert.Equal("S4", RoadSegmentCategory.SecondaryRoadType4);
        }

        [Fact]
        public void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentCategory.Unknown,
                    RoadSegmentCategory.NotApplicable,
                    RoadSegmentCategory.MainRoad,
                    RoadSegmentCategory.LocalRoad,
                    RoadSegmentCategory.LocalRoadType1,
                    RoadSegmentCategory.LocalRoadType2,
                    RoadSegmentCategory.LocalRoadType3,
                    RoadSegmentCategory.PrimaryRoadI,
                    RoadSegmentCategory.PrimaryRoadII,
                    RoadSegmentCategory.PrimaryRoadIIType1,
                    RoadSegmentCategory.PrimaryRoadIIType2,
                    RoadSegmentCategory.PrimaryRoadIIType3,
                    RoadSegmentCategory.PrimaryRoadIIType4,
                    RoadSegmentCategory.SecondaryRoad,
                    RoadSegmentCategory.SecondaryRoadType1,
                    RoadSegmentCategory.SecondaryRoadType2,
                    RoadSegmentCategory.SecondaryRoadType3,
                    RoadSegmentCategory.SecondaryRoadType4
                },
                RoadSegmentCategory.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var sut = RoadSegmentCategory.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            Assert.NotNull(RoadSegmentCategory.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentCategory.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<string>(_knownValues));
            var value = _fixture.Create<string>();
            var result = RoadSegmentCategory.TryParse(value, out RoadSegmentCategory parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToString());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<string>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentCategory.TryParse(value, out RoadSegmentCategory parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
