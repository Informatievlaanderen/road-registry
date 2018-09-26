namespace RoadRegistry.Model
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class PositionTests
    {
        private readonly Fixture _fixture;

        public PositionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeDoubleBehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<Double>(_fixture),
                new ExplicitConversionMethodAssertion<Double>(_fixture),
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
            ).Verify(typeof(Position));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<Double>();
            var sut = new Position(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }

        [Theory]
        [InlineData(1.0, 2.0, -1)]
        [InlineData(2.0, 1.0, 1)]
        [InlineData(1.0, 1.0, 0)]
        public void CompareToReturnsExpectedResult(Double left, Double right, Int32 expected)
        {
            var sut = new Position(left);

            var result = sut.CompareTo(new Position(right));

            Assert.Equal(expected, result);
        }
    }
}