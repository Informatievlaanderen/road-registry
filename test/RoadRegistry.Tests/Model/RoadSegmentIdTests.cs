namespace RoadRegistry.Model
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentIdTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentIdTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<int>(_fixture),
                new ExplicitConversionMethodAssertion<int>(_fixture),
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
            ).Verify(typeof(RoadSegmentId));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<int>();
            var sut = new RoadSegmentId(value);

            Assert.Equal("RS-" + value, sut.ToString());
        }

        [Theory]
        [InlineData(1, 2, -1)]
        [InlineData(2, 1, 1)]
        public void CompareToReturnsExpectedResult(int left, int right, int expected)
        {
            var sut = new RoadSegmentId(left);

            var result = sut.CompareTo(new RoadSegmentId(right));

            Assert.Equal(expected, result);
        }
    }
}
