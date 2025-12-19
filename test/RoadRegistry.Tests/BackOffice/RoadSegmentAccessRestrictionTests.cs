namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentAccessRestrictionTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentAccessRestrictionTests()
    {
        _fixture = new Fixture();
        _knownValues = Array.ConvertAll(RoadSegmentAccessRestriction.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentAccessRestriction.PublicRoad,
                RoadSegmentAccessRestriction.PhysicallyImpossible,
                RoadSegmentAccessRestriction.LegallyForbidden,
                RoadSegmentAccessRestriction.PrivateRoad,
                RoadSegmentAccessRestriction.Seasonal,
                RoadSegmentAccessRestriction.Toll
            },
            RoadSegmentAccessRestriction.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentAccessRestriction.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentAccessRestriction.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentAccessRestriction.CanParse(null));
    }

    [Fact]
    public void LegallyForbiddenReturnsExpectedResult()
    {
        Assert.Equal("LegallyForbidden", RoadSegmentAccessRestriction.LegallyForbidden);
    }

    [Fact]
    public void LegallyForbiddenTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RoadSegmentAccessRestriction.LegallyForbidden.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentAccessRestriction.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentAccessRestriction.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentAccessRestriction.Parse(null));
    }

    [Fact]
    public void PhysicallyImpossibleReturnsExpectedResult()
    {
        Assert.Equal("PhysicallyImpossible", RoadSegmentAccessRestriction.PhysicallyImpossible);
    }

    [Fact]
    public void PhysicallyImpossibleTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadSegmentAccessRestriction.PhysicallyImpossible.Translation.Identifier);
    }

    [Fact]
    public void PrivateRoadReturnsExpectedResult()
    {
        Assert.Equal("PrivateRoad", RoadSegmentAccessRestriction.PrivateRoad);
    }

    [Fact]
    public void PrivateRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(4, RoadSegmentAccessRestriction.PrivateRoad.Translation.Identifier);
    }

    [Fact]
    public void PublicRoadReturnsExpectedResult()
    {
        Assert.Equal("PublicRoad", RoadSegmentAccessRestriction.PublicRoad);
    }

    [Fact]
    public void PublicRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadSegmentAccessRestriction.PublicRoad.Translation.Identifier);
    }

    [Fact]
    public void SeasonalReturnsExpectedResult()
    {
        Assert.Equal("Seasonal", RoadSegmentAccessRestriction.Seasonal);
    }

    [Fact]
    public void SeasonalTranslationReturnsExpectedResult()
    {
        Assert.Equal(5, RoadSegmentAccessRestriction.Seasonal.Translation.Identifier);
    }

    [Fact]
    public void TollReturnsExpectedResult()
    {
        Assert.Equal("Toll", RoadSegmentAccessRestriction.Toll);
    }

    [Fact]
    public void TollTranslationReturnsExpectedResult()
    {
        Assert.Equal(6, RoadSegmentAccessRestriction.Toll.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentAccessRestriction.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentAccessRestriction.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentAccessRestriction.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentAccessRestriction.TryParse(null, out _));
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
        ).Verify(typeof(RoadSegmentAccessRestriction));
    }
}
