namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class ReasonTests
{
    private readonly Fixture _fixture;

    public ReasonTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeReason();
    }

    [Fact]
    public void NoneGetHashCodeReturnsExpectedResult()
    {
        Assert.Equal(0, Reason.None.GetHashCode());
    }

    [Fact]
    public void NoneToStringReturnsExpectedResult()
    {
        Assert.Null(Reason.None.ToString());
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = new string(
            (char)new Random().Next(97, 123), // a-z
            new Random().Next(1, Reason.MaxLength + 1)
        );
        var sut = new Reason(value);

        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan18Chars()
    {
        const int length = Reason.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Reason(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var customizedString = new Fixture();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                new string(
                    (char)new Random().Next(97, 123), // a-z
                    generator.Next(1, Reason.MaxLength + 1)
                )
            ));
        new CompositeIdiomaticAssertion(
            new GuardClauseAssertion(
                _fixture,
                new CompositeBehaviorExpectation(
                    new NullReferenceBehaviorExpectation(),
                    new EmptyStringBehaviorExpectation()
                )
            ),
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
        ).Verify(typeof(Reason));
    }
}
