namespace RoadRegistry.Model
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadNodeIdTests
    {
        private readonly Fixture _fixture;

        public RoadNodeIdTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeInt64BehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<Int64>(_fixture),
                new ExplicitConversionMethodAssertion<Int64>(_fixture),
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
                new ComparableCompareToSelfAssertion(_fixture)
            ).Verify(typeof(RoadNodeId));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<Int64>();
            var sut = new RoadNodeId(value);

            Assert.Equal("RN-" + value, sut.ToString());
        }

        [Theory]
        [InlineData(1L, 2L, -1)]
        [InlineData(2L, 1L, 1)]
        public void CompareToReturnsExpectedResult(Int64 left, Int64 right, Int32 expected)
        {
            var sut = new RoadNodeId(left);
            
            var result = sut.CompareTo(new RoadNodeId(right));

            Assert.Equal(expected, result);
        }
    }
}
