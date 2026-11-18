namespace RoadRegistry.Tests.BackOffice;

using System.Globalization;
using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class StreetNameLocalIdTests
{
    private readonly Fixture _fixture;

    public StreetNameLocalIdTests()
    {
        _fixture = FixtureFactory.Create();
    }

    [Fact]
    public void CtorValueCanNotBeNegative()
    {
        new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()).Verify(
            Constructors.Select(() => new StreetNameLocalId(0)));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>();
        var sut = new StreetNameLocalId(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), sut.ToString());
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("14291", 14291)]
    [InlineData("-8", -8)]
    [InlineData("-9", -9)]
    public void TryParseAcceptsAValidId(string value, int expected)
    {
        Assert.True(StreetNameLocalId.TryParse(value, out var parsed));
        Assert.Equal(new StreetNameLocalId(expected), parsed);
        Assert.Equal(new StreetNameLocalId(expected), StreetNameLocalId.Parse(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("niet gekend")]
    public void TryParseRejectsAnInvalidId(string? value)
    {
        Assert.False(StreetNameLocalId.TryParse(value, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("niet gekend")]
    public void ParseThrowsOnAnInvalidId(string value)
    {
        Assert.Throws<FormatException>(() => StreetNameLocalId.Parse(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<int>(_fixture),
            new ExplicitConversionMethodAssertion<int>(_fixture),
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
        ).Verify(typeof(StreetNameLocalId));
    }
}
