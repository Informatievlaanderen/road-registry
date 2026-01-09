namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class OrganizationOvoCodeTests
{
    private readonly Fixture _fixture;

    public OrganizationOvoCodeTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeOrganizationOvoCode();
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResult()
    {
        var value = _fixture.Create<OrganizationOvoCode>().ToString();

        Assert.True(OrganizationOvoCode.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueLongerThan9Chars()
    {
        const int length = OrganizationOvoCode.Length + 1;

        var value = new string((char)new Random().Next(97, 123), length);

        Assert.False(OrganizationOvoCode.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueDoesntStartWithOvo()
    {
        var digits = new Random().Next(OrganizationOvoCode.MinDigitsValue, OrganizationOvoCode.MaxDigitsValue + 1);
        var value = $"XXX{digits:000000}";

        Assert.False(OrganizationOvoCode.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueDoesntEndWith6Digits()
    {
        var digits = new Random().Next(OrganizationOvoCode.MinDigitsValue, OrganizationOvoCode.MaxDigitsValue + 1);
        var value = $"OVO{digits:00000}X";

        Assert.False(OrganizationOvoCode.AcceptsValue(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
    {
        Assert.False(OrganizationOvoCode.AcceptsValue(value));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var digits = new Random().Next(OrganizationOvoCode.MinDigitsValue, OrganizationOvoCode.MaxDigitsValue + 1);
        var value = $"OVO{digits:000000}";

        var sut = new OrganizationOvoCode(value);
        Assert.Equal(value, sut.ToString());

        sut = new OrganizationOvoCode(digits);
        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan9Chars()
    {
        const int length = OrganizationOvoCode.Length + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrganizationOvoCode(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = FixtureFactory.Create();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                $"OVO{generator.Next(OrganizationOvoCode.MinDigitsValue, OrganizationOvoCode.MaxDigitsValue + 1):000000}"
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
        ).Verify(typeof(OrganizationOvoCode));

        new GuardClauseAssertion(
            _fixture,
            new CompositeBehaviorExpectation(
                new NullReferenceBehaviorExpectation(),
                new EmptyStringBehaviorExpectation()
            )
        ).Verify(Constructors.Select(() => new OrganizationOvoCode(null)));
    }
}
