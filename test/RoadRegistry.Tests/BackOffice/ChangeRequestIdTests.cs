namespace RoadRegistry.BackOffice
{
    using System;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using RoadRegistry.Framework.Assertions;
    using Xunit;

    public class ChangeRequestIdTests
    {
        private readonly Fixture _fixture;

        public ChangeRequestIdTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<Guid>(_fixture),
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
            ).Verify(typeof(ChangeRequestId));
        }

        [Fact]
        public void CtorValueCanNotBeEmpty()
        {
            new GuardClauseAssertion(
                _fixture,
                new EmptyGuidBehaviorExpectation()
            ).Verify(Constructors.Select(() => new ChangeRequestId(Guid.Empty)));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = Guid.NewGuid();
            var sut = new ChangeRequestId(value);

            Assert.Equal(value.ToString("N"), sut.ToString());
        }

        [Fact]
        public void AcceptsReturnsExceptedResult()
        {
            Assert.False(ChangeRequestId.Accepts(Guid.Empty));
            Assert.True(ChangeRequestId.Accepts(Guid.NewGuid()));
        }
    }
}
