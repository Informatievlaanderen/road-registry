namespace RoadRegistry
{
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
                //TODO:
                // implicit conversion to Int64
                // explicit conversion to Int64
                // equality and inequality properly overridden
                // two different values do not equal assertion
                // same value does equal assertion
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
                ).Verify(typeof(RoadNodeId));
        }
    }
}
