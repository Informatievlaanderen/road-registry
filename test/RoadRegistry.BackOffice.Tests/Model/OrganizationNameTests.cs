namespace RoadRegistry.BackOffice.Model
{
    using System;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Framework;
    using Xunit;

    public class OrganizationNameTests
    {
        private readonly Fixture _fixture;

        public OrganizationNameTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeOrganizationName();
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.Customize<string>(customization =>
                customization.FromFactory(generator =>
                    new string(
                        (char) new Random().Next(97, 123), // a-z
                        generator.Next(1, OrganizationName.MaxLength + 1)
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
            ).Verify(typeof(OrganizationName));

            new GuardClauseAssertion(
                _fixture,
                new CompositeBehaviorExpectation(
                    new NullReferenceBehaviorExpectation(),
                    new EmptyStringBehaviorExpectation()
                )
            ).Verify(Constructors.Select(() => new OrganizationName(null)));
        }

        [Fact]
        public void AcceptsValueReturnsExpectedResultWhenValueLongerThan64Chars()
        {
            const int length = OrganizationName.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);

            Assert.False(OrganizationName.AcceptsValue(value));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
        {
            Assert.False(OrganizationName.AcceptsValue(value));
        }

        [Fact]
        public void AcceptsValueReturnsExpectedResult()
        {
            var value = _fixture.Create<OrganizationName>().ToString();

            Assert.True(OrganizationName.AcceptsValue(value));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new string(
                (char) new Random().Next(97, 123), // a-z
                new Random().Next(1, OrganizationName.MaxLength + 1)
            );
            var sut = new OrganizationName(value);

            Assert.Equal(value, sut.ToString());
        }

        [Fact]
        public void ValueCanNotBeLongerThan64Chars()
        {
            const int length = OrganizationName.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrganizationName(value));
        }
    }
}
