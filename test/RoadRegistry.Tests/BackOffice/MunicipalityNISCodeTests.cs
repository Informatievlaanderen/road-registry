namespace RoadRegistry.BackOffice
{
    using System;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using RoadRegistry.Framework.Assertions;
    using Xunit;

    public class MunicipalityNISCodeTests
    {
        private readonly Fixture _fixture;

        public MunicipalityNISCodeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeMunicipalityNISCode();
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.Customize<string>(customization =>
                customization.FromFactory(generator =>
                    new string(
                        (char) new Random().Next(97, 123), // a-z
                        MunicipalityNISCode.ExactLength
                    )
                ));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<string>(
                    new CompositeSpecimenBuilder(customizedString, _fixture)),
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
            ).Verify(typeof(MunicipalityNISCode));

            new GuardClauseAssertion(
                _fixture,
                new CompositeBehaviorExpectation(
                    new NullReferenceBehaviorExpectation()
                )
            ).Verify(Constructors.Select(() => new MunicipalityNISCode(null)));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new string(
                (char) new Random().Next(97, 123), // a-z
                MunicipalityNISCode.ExactLength
            );
            var sut = new MunicipalityNISCode(value);

            Assert.Equal(value, sut.ToString());
        }
    }
}
