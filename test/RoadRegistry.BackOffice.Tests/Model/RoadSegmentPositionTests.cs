namespace RoadRegistry.BackOffice.Model
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Framework;
    using Xunit;

    public class RoadSegmentPositionTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentPositionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeDoubleBehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<decimal>(_fixture),
                new ExplicitConversionMethodAssertion<decimal>(_fixture),
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
                new GetHashCodeSuccessiveAssertion(_fixture),
                new ComparableCompareToSelfAssertion(_fixture),
                new LessThanOperatorCompareToSelfAssertion(_fixture),
                new LessThanOrEqualOperatorCompareToSelfAssertion(_fixture),
                new GreaterThanOperatorCompareToSelfAssertion(_fixture),
                new GreaterThanOrEqualOperatorCompareToSelfAssertion(_fixture)
            ).Verify(typeof(RoadSegmentPosition));
        }

        [Fact]
        public void FromDoubleReturnsExpectedResult()
        {
            var value = _fixture.Create<double>();

            var result = RoadSegmentPosition.FromDouble(value);

            Assert.Equal(new RoadSegmentPosition(Convert.ToDecimal(value)), result);
        }

        [Fact]
        public void ZeroReturnsExpectedValue()
        {
            Assert.Equal(0.0m, RoadSegmentPosition.Zero.ToDecimal());
        }

        [Fact]
        public void ToDoubleReturnsExpectedValue()
        {
            var value = _fixture.Create<decimal>();
            var sut =  new RoadSegmentPosition(value);

            var result = sut.ToDouble();

            Assert.Equal(decimal.ToDouble(value), result);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<decimal>();
            var sut = new RoadSegmentPosition(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }

        [Theory]
        [InlineData(1.0, 2.0, -1)]
        [InlineData(2.0, 1.0, 1)]
        [InlineData(1.0, 1.0, 0)]
        public void CompareToReturnsExpectedResult(double left, double right, int expected)
        {
            var sut = new RoadSegmentPosition(new decimal(left));

            var result = sut.CompareTo(new RoadSegmentPosition(new decimal(right)));

            Assert.Equal(expected, result);
        }
    }
}
