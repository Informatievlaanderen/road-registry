namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentStatusTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentStatusTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(RoadSegmentStatus.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentStatus.Unknown,
                RoadSegmentStatus.PermitRequested,
                RoadSegmentStatus.PermitGranted,
                RoadSegmentStatus.UnderConstruction,
                RoadSegmentStatus.InUse,
                RoadSegmentStatus.OutOfUse
            },
            RoadSegmentStatus.All);
    }

    [Fact]
    public void BuildingPermitGrantedReturnsExpectedResult()
    {
        Assert.Equal("BuildingPermitGranted", RoadSegmentStatus.PermitGranted);
    }

    [Fact]
    public void BuildingPermitGrantedTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadSegmentStatus.PermitGranted.Translation.Identifier);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentStatus.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentStatus.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentStatus.CanParse(null));
    }

    [Fact]
    public void InUseReturnsExpectedResult()
    {
        Assert.Equal("InUse", RoadSegmentStatus.InUse);
    }

    [Fact]
    public void InUseTranslationReturnsExpectedResult()
    {
        Assert.Equal(4, RoadSegmentStatus.InUse.Translation.Identifier);
    }

    [Fact]
    public void OutOfUseReturnsExpectedResult()
    {
        Assert.Equal("OutOfUse", RoadSegmentStatus.OutOfUse);
    }

    [Fact]
    public void OutOfUseTranslationReturnsExpectedResult()
    {
        Assert.Equal(5, RoadSegmentStatus.OutOfUse.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentStatus.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentStatus.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentStatus.Parse(null));
    }

    [Fact]
    public void PermitRequestedReturnsExpectedResult()
    {
        Assert.Equal("PermitRequested", RoadSegmentStatus.PermitRequested);
    }

    [Fact]
    public void PermitRequestedTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadSegmentStatus.PermitRequested.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentStatus.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentStatus.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentStatus.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentStatus.TryParse(null, out _));
    }

    [Fact]
    public void UnderConstructionReturnsExpectedResult()
    {
        Assert.Equal("UnderConstruction", RoadSegmentStatus.UnderConstruction);
    }

    [Fact]
    public void UnderConstructionTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RoadSegmentStatus.UnderConstruction.Translation.Identifier);
    }

    [Fact]
    public void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", RoadSegmentStatus.Unknown);
    }

    [Fact]
    public void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal(-8, RoadSegmentStatus.Unknown.Translation.Identifier);
    }

    [Fact]
    public void VerifyBehavior()
    {
        _fixture.Customizations.Add(
            new FiniteSequenceGenerator<string>(_knownValues));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string?>(_fixture),
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
        ).Verify(typeof(RoadSegmentStatus));
    }
}
