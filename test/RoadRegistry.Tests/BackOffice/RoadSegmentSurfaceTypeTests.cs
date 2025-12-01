namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentSurfaceTypeTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentSurfaceTypeTests()
    {
        _fixture = new Fixture();
        _knownValues = Array.ConvertAll(RoadSegmentSurfaceType.All, type => type.ToString());
    }

    [Fact]
    public static void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentSurfaceType.NotApplicable,
                RoadSegmentSurfaceType.Unknown,
                RoadSegmentSurfaceType.SolidSurface,
                RoadSegmentSurfaceType.LooseSurface
            },
            RoadSegmentSurfaceType.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentSurfaceType.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentSurfaceType.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentSurfaceType.CanParse(null));
    }

    [Fact]
    public static void LooseSurfaceReturnsExpectedResult()
    {
        Assert.Equal("LooseSurface", RoadSegmentSurfaceType.LooseSurface);
    }

    [Fact]
    public static void LooseSurfaceTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadSegmentSurfaceType.LooseSurface.Translation.Identifier);
    }

    [Fact]
    public static void NotApplicableReturnsExpectedResult()
    {
        Assert.Equal("NotApplicable", RoadSegmentSurfaceType.NotApplicable);
    }

    [Fact]
    public static void NotApplicableTranslationReturnsExpectedResult()
    {
        Assert.Equal(-9, RoadSegmentSurfaceType.NotApplicable.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentSurfaceType.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentSurfaceType.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentSurfaceType.Parse(null));
    }

    [Fact]
    public static void SolidSurfaceReturnsExpectedResult()
    {
        Assert.Equal("SolidSurface", RoadSegmentSurfaceType.SolidSurface);
    }

    [Fact]
    public static void SolidSurfaceTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadSegmentSurfaceType.SolidSurface.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentSurfaceType.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentSurfaceType.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentSurfaceType.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentSurfaceType.TryParse(null, out _));
    }

    [Fact]
    public static void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", RoadSegmentSurfaceType.Unknown);
    }

    [Fact]
    public static void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal(-8, RoadSegmentSurfaceType.Unknown.Translation.Identifier);
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
        ).Verify(typeof(RoadSegmentSurfaceType));
    }
}
