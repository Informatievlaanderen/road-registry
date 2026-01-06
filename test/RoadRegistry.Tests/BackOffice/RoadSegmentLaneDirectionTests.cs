namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentLaneDirectionTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentLaneDirectionTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(RoadSegmentLaneDirection.All, type => type.ToString());
    }

    [Fact]
    public static void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentLaneDirection.NotApplicable,
                RoadSegmentLaneDirection.Unknown,
                RoadSegmentLaneDirection.Forward,
                RoadSegmentLaneDirection.Backward,
                RoadSegmentLaneDirection.Independent
            },
            RoadSegmentLaneDirection.All);
    }

    [Fact]
    public static void BackwardReturnsExpectedResult()
    {
        Assert.Equal("Backward", RoadSegmentLaneDirection.Backward);
    }

    [Fact]
    public static void BackwardTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadSegmentLaneDirection.Backward.Translation.Identifier);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentLaneDirection.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentLaneDirection.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentLaneDirection.CanParse(null));
    }

    [Fact]
    public static void ForwardReturnsExpectedResult()
    {
        Assert.Equal("Forward", RoadSegmentLaneDirection.Forward);
    }

    [Fact]
    public static void ForwardTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadSegmentLaneDirection.Forward.Translation.Identifier);
    }

    [Fact]
    public static void IndependentReturnsExpectedResult()
    {
        Assert.Equal("Independent", RoadSegmentLaneDirection.Independent);
    }

    [Fact]
    public static void IndependentTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RoadSegmentLaneDirection.Independent.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentLaneDirection.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentLaneDirection.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentLaneDirection.Parse(null));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentLaneDirection.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentLaneDirection.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentLaneDirection.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentLaneDirection.TryParse(null, out _));
    }

    [Fact]
    public static void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", RoadSegmentLaneDirection.Unknown);
    }

    [Fact]
    public static void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal(-8, RoadSegmentLaneDirection.Unknown.Translation.Identifier);
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
        ).Verify(typeof(RoadSegmentLaneDirection));
    }
}
