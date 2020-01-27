namespace RoadRegistry.BackOffice
{
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Framework;
    using Xunit;

    public class RoadSegmentNumberedRoadOrdinalTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentNumberedRoadOrdinalTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
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
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(RoadSegmentNumberedRoadOrdinal));

            new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation())
                .Verify(Constructors.Select(() => new RoadSegmentNumberedRoadOrdinal(0)));
        }

        [Theory]
        [InlineData(int.MinValue, false)]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(int.MaxValue, true)]
        public void AcceptsReturnsExpectedResult(int value, bool expected)
        {
            var result = RoadSegmentNumberedRoadOrdinal.Accepts(value);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<int>();
            var sut = new RoadSegmentNumberedRoadOrdinal(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }
    }
}
