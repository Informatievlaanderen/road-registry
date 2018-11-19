namespace RoadRegistry.Model
{
    using System;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class MaintenanceAuthorityIdTests
    {
        private readonly Fixture _fixture;

        public MaintenanceAuthorityIdTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeMaintenanceAuthorityId();
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
                    )
                ),
                new ImplicitConversionOperatorAssertion<string>(
                    () => new string(
                        (char) new Random().Next(97, 123), // a-z
                        new Random().Next(1, MaintenanceAuthorityId.MaxLength + 1)
                    ),
                    value => new MaintenanceAuthorityId(value)
                ),
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
            var value = new string(
                (char) new Random().Next(97, 123), // a-z
                new Random().Next(1, MaintenanceAuthorityId.MaxLength + 1)
            );
            var sut = new MaintenanceAuthorityId(value);

            Assert.Equal(value, sut.ToString());
        }

        [Fact]
        public void ValueCanNotBeLongerThan18Chars()
        {
            const int length = MaintenanceAuthorityId.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MaintenanceAuthorityId(value));
        }
    }
}
