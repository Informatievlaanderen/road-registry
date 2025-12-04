namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class GradeSeparatedJunctionTypeTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public GradeSeparatedJunctionTypeTests()
    {
        _fixture = new Fixture();
        _knownValues = Array.ConvertAll(GradeSeparatedJunctionType.All, type => type.ToString());
    }

    [Fact]
    public static void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                GradeSeparatedJunctionType.Unknown,
                GradeSeparatedJunctionType.Tunnel,
                GradeSeparatedJunctionType.Bridge
            },
            GradeSeparatedJunctionType.All);
    }

    [Fact]
    public static void BridgeReturnsExpectedResult()
    {
        Assert.Equal("Bridge", GradeSeparatedJunctionType.Bridge);
    }

    [Fact]
    public static void BridgeTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, GradeSeparatedJunctionType.Bridge.Translation.Identifier);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = GradeSeparatedJunctionType.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = GradeSeparatedJunctionType.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => GradeSeparatedJunctionType.CanParse(null));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => GradeSeparatedJunctionType.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(GradeSeparatedJunctionType.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => GradeSeparatedJunctionType.Parse(null));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = GradeSeparatedJunctionType.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = GradeSeparatedJunctionType.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = GradeSeparatedJunctionType.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => GradeSeparatedJunctionType.TryParse(null, out _));
    }

    [Fact]
    public static void TunnelReturnsExpectedResult()
    {
        Assert.Equal("Tunnel", GradeSeparatedJunctionType.Tunnel);
    }

    [Fact]
    public static void TunnelTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, GradeSeparatedJunctionType.Tunnel.Translation.Identifier);
    }

    [Fact]
    public static void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", GradeSeparatedJunctionType.Unknown);
    }

    [Fact]
    public static void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal(-8, GradeSeparatedJunctionType.Unknown.Translation.Identifier);
    }

    [Fact]
    public void VerifyBehavior()
    {
        _fixture.Customizations.Add(
            new FiniteSequenceGenerator<string>(_knownValues));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string>(_fixture),
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
        ).Verify(typeof(GradeSeparatedJunctionType));
    }
}
