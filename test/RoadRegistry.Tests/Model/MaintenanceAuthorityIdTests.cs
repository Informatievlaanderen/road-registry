namespace RoadRegistry.Model
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class MaintenanceAuthorityIdTests
    {
        private readonly Fixture _fixture;

        public MaintenanceAuthorityIdTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(
                    _fixture,
                    new CompositeBehaviorExpectation(
                        new NullReferenceBehaviorExpectation(),
                        new EmptyStringBehaviorExpectation()
                    )),
                new ImplicitConversionOperatorAssertion<string>(_fixture),
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
            ).Verify(typeof(MaintenanceAuthorityId));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<string>();
            var sut = new MaintenanceAuthorityId(value);

            Assert.Equal(value, sut.ToString());
        }
    }
}
