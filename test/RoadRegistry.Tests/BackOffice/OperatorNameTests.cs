namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class OperatorNameTests
{
    private readonly Fixture _fixture;

    public OperatorNameTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeOperatorName();
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResult()
    {
        var value = _fixture.Create<OperatorName>().ToString();

        Assert.True(OperatorName.AcceptsValue(value));
    }

    [Fact]
    public void AcceptsValueReturnsExpectedResultWhenValueLongerThan64Chars()
    {
        const int length = OperatorName.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);

        Assert.False(OperatorName.AcceptsValue(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AcceptsValueReturnsExpectedResultWhenValueNullOrEmpty(string value)
    {
        Assert.False(OperatorName.AcceptsValue(value));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = new string(
            (char)new Random().Next(97, 123), // a-z
            new Random().Next(1, OperatorName.MaxLength + 1)
        );
        var sut = new OperatorName(value);

        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan18Chars()
    {
        const int length = OperatorName.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new OperatorName(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = new Fixture();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                new string(
                    (char)new Random().Next(97, 123), // a-z
                    generator.Next(1, OperatorName.MaxLength + 1)
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
        ).Verify(typeof(OperatorName));

        new GuardClauseAssertion(
            _fixture,
            new CompositeBehaviorExpectation(
                new NullReferenceBehaviorExpectation(),
                new EmptyStringBehaviorExpectation()
            )
        ).Verify(Constructors.Select(() => new OperatorName(null)));
    }
}
