namespace RoadRegistry.Model
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadNetworkRevisionTests
    {
        private readonly Fixture _fixture;

        public RoadNetworkRevisionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()),
                new ImplicitConversionOperatorAssertion<int>(
                    () => _fixture.Create<int>(),
                    value => new RoadNetworkRevision(value)),
                new ExplicitConversionMethodAssertion<int>(
                    () => _fixture.Create<int>(),
                    value => new RoadNetworkRevision(value)),
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
            ).Verify(typeof(RoadNetworkRevision));
        }
    }
}
