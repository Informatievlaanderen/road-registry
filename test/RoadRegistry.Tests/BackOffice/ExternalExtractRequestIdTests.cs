namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class ExternalExtractRequestIdTests
{
    private readonly Fixture _fixture;

    public ExternalExtractRequestIdTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeExternalExtractRequestId();
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResult()
    {
        var value = _fixture.Create<ExternalExtractRequestId>().ToString();

        Assert.True(ExternalExtractRequestId.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueLongerThan256Chars()
    {
        const int length = ExternalExtractRequestId.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);

        Assert.False(ExternalExtractRequestId.AcceptsValue(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
    {
        Assert.False(OrganizationId.AcceptsValue(value));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = new string(
            (char)new Random().Next(97, 123), // a-z
            new Random().Next(1, ExternalExtractRequestId.MaxLength + 1)
        );
        var sut = new ExternalExtractRequestId(value);

        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan256Chars()
    {
        const int length = ExternalExtractRequestId.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExternalExtractRequestId(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = FixtureFactory.Create();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                new string(
                    (char)new Random().Next(97, 123), // a-z
                    generator.Next(1, ExternalExtractRequestId.MaxLength + 1)
                )
            ));
        var fixture = new CompositeSpecimenBuilder(customizedString, _fixture);
        var constructor = Constructors.Select(() => new ExternalExtractRequestId(null));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string?>(fixture),
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
        ).Verify(typeof(ExternalExtractRequestId));

        new GuardClauseAssertion(
            _fixture,
            new CompositeBehaviorExpectation(
                new NullReferenceBehaviorExpectation(),
                new EmptyStringBehaviorExpectation()
            )
        ).Verify(Constructors.Select(() => new ExternalExtractRequestId(null)));
    }
}
