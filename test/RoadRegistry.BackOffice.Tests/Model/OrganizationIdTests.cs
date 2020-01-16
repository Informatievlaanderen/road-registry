namespace RoadRegistry.BackOffice.Model
{
    using System;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Framework;
    using Xunit;

    public class OrganizationIdTests
    {
        private readonly Fixture _fixture;

        public OrganizationIdTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeOrganizationId();
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.Customize<string>(customization =>
                customization.FromFactory(generator =>
                    new string(
                        (char) new Random().Next(97, 123), // a-z
                        generator.Next(1, OrganizationId.MaxLength + 1)
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
            ).Verify(typeof(OrganizationId));

            new GuardClauseAssertion(
                _fixture,
                new CompositeBehaviorExpectation(
                    new NullReferenceBehaviorExpectation(),
                    new EmptyStringBehaviorExpectation()
                )
            ).Verify(Constructors.Select(() => new OrganizationId(null)));
        }

        [Fact]
        public void AcceptsValueReturnsExpectedResultWhenValueLongerThan18Chars()
        {
            const int length = OrganizationId.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);

            Assert.False(OrganizationId.AcceptsValue(value));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
        {
            Assert.False(OrganizationId.AcceptsValue(value));
        }

        [Fact]
        public void AcceptsValueReturnsExpectedResult()
        {
            var value = _fixture.Create<OrganizationId>().ToString();

            Assert.True(OrganizationId.AcceptsValue(value));
        }

        [Fact]
        public void UnknownReturnsExpectedValue()
        {
            var sut = OrganizationId.Unknown;

            Assert.Equal("-8", sut.ToString());
        }

        [Fact]
        public void OtherReturnsExpectedValue()
        {
            var sut = OrganizationId.Other;

            Assert.Equal("-7", sut.ToString());
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new string(
                (char) new Random().Next(97, 123), // a-z
                new Random().Next(1, OrganizationId.MaxLength + 1)
            );
            var sut = new OrganizationId(value);

            Assert.Equal(value, sut.ToString());
        }

        [Fact]
        public void ValueCanNotBeLongerThan18Chars()
        {
            const int length = OrganizationId.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrganizationId(value));
        }
    }
}
