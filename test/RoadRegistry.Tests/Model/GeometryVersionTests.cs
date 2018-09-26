namespace RoadRegistry.Model
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class GeometryVersionTests
    {
        private readonly Fixture _fixture;

        public GeometryVersionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<Int32>(_fixture),
                new ExplicitConversionMethodAssertion<Int32>(_fixture),
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
            ).Verify(typeof(GeometryVersion));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<Int32>();
            var sut = new GeometryVersion(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }

        [Theory]
        [InlineData(1, 2, -1)]
        [InlineData(2, 1, 1)]
        [InlineData(1, 1, 0)]
        public void CompareToReturnsExpectedResult(Int32 left, Int32 right, Int32 expected)
        {
            var sut = new GeometryVersion(left);

            var result = sut.CompareTo(new GeometryVersion(right));

            Assert.Equal(expected, result);
        }
    }
}