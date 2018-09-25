namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentGeometryDrawMethodTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentGeometryDrawMethodTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentGeometryDrawMethod.All, type => type.ToInt32());
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
            ).Verify(typeof(RoadSegmentGeometryDrawMethod));
        }

        [Fact]
        public static void OutlinedReturnsExpectedResult()
        {
            Assert.Equal(1, RoadSegmentGeometryDrawMethod.Outlined);
        }

        [Fact]
        public static void MeasuredReturnsExpectedResult()
        {
            Assert.Equal(2, RoadSegmentGeometryDrawMethod.Measured);
        }

        [Fact]
        public static void Measured_according_to_GRB_specificationsReturnsExpectedResult()
        {
            Assert.Equal(3, RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentGeometryDrawMethod.Outlined,
                    RoadSegmentGeometryDrawMethod.Measured,
                    RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications
                },
                RoadSegmentGeometryDrawMethod.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentGeometryDrawMethod.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentGeometryDrawMethod.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentGeometryDrawMethod.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentGeometryDrawMethod.TryParse(value, out RoadSegmentGeometryDrawMethod parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentGeometryDrawMethod.TryParse(value, out RoadSegmentGeometryDrawMethod parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
