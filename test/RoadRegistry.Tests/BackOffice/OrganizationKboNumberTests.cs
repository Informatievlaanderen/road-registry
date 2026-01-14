namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class OrganizationKboNumberTests
{
    private readonly Fixture _fixture;

    public OrganizationKboNumberTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeOrganizationKboNumber();
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResult()
    {
        var value = _fixture.Create<OrganizationKboNumber>().ToString();

        Assert.True(OrganizationKboNumber.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueLongerThan10Chars()
    {
        const int length = OrganizationKboNumber.Length + 1;

        var value = new string((char)new Random().Next(97, 123), length);

        Assert.False(OrganizationKboNumber.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueDoesntOnlyHaveDigits()
    {
        var digits = new Random().Next(1, 99999);
        var value = $"{digits:00000}{digits:0000}x";

        Assert.False(OrganizationKboNumber.AcceptsValue(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
    {
        Assert.False(OrganizationKboNumber.AcceptsValue(value));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var digits = new Random().Next(1, 99999);
        var value = $"{digits:00000}{digits:00000}";

        var sut = new OrganizationKboNumber(value);
        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan10Chars()
    {
        const int length = OrganizationKboNumber.Length + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrganizationKboNumber(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = FixtureFactory.Create();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                $"{generator.Next(1, 99999):00000}{generator.Next(1, 99999):00000}"
            ));

        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string?>(
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
        ).Verify(typeof(OrganizationKboNumber));

        new GuardClauseAssertion(
            _fixture,
            new CompositeBehaviorExpectation(
                new NullReferenceBehaviorExpectation(),
                new EmptyStringBehaviorExpectation()
            )
        ).Verify(Constructors.Select(() => new OrganizationKboNumber(null)));
    }
}
