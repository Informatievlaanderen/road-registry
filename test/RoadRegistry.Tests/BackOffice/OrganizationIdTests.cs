namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class OrganizationIdTests
{
    private readonly Fixture _fixture;

    public OrganizationIdTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeOrganizationId();
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResult()
    {
        var value = _fixture.Create<OrganizationId>().ToString();

        Assert.True(OrganizationId.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueLongerThan18Chars()
    {
        const int length = OrganizationId.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);

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
    public void OtherReturnsExpectedValue()
    {
        var sut = OrganizationId.Other;

        Assert.Equal("-7", sut.ToString());
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = new string(
            (char)new Random().Next(97, 123), // a-z
            new Random().Next(1, OrganizationId.MaxLength + 1)
        );
        var sut = new OrganizationId(value);

        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void UnknownReturnsExpectedValue()
    {
        var sut = OrganizationId.Unknown;

        Assert.Equal("-8", sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan18Chars()
    {
        const int length = OrganizationId.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrganizationId(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = new Fixture();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                new string(
                    (char)new Random().Next(97, 123), // a-z
                    generator.Next(1, OrganizationId.MaxLength + 1)
                )
            ));
        var fixture = new CompositeSpecimenBuilder(customizedString, _fixture);
        var constructor = Constructors.Select(() => new OrganizationId(null));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string>(fixture),
            new EquatableEqualsSelfAssertion(_fixture),
            new ConstructorBasedEquatableEqualsOtherAssertion(fixture, constructor),
            new EqualityOperatorEqualsSelfAssertion(_fixture),
            new ConstructorBasedEqualityOperatorEqualsOtherAssertion(fixture, constructor),
            new InequalityOperatorEqualsSelfAssertion(_fixture),
            new ConstructorBasedInequalityOperatorEqualsOtherAssertion(fixture, constructor),
            new EqualsNewObjectAssertion(_fixture),
            new EqualsNullAssertion(_fixture),
            new EqualsSelfAssertion(_fixture),
            new ConstructorBasedEqualsOtherAssertion(fixture, constructor),
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

    [Theory]
    [InlineData(false, "-6")]
    [InlineData(true, "-7")]
    [InlineData(true, "-8")]
    [InlineData(false, "-9")]
    [InlineData(false, "0")]
    [InlineData(false, "1")]
    [InlineData(false, "OVO123456")]
    public void AcceptsValueReturnsExpectedResultWhenValueIsSystemValue(bool expected, string value)
    {
        Assert.Equal(expected, OrganizationId.IsSystemValue(new OrganizationId(value)));
    }
}
