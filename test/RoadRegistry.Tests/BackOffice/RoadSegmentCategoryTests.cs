namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentCategoryTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentCategoryTests()
    {
        _fixture = new Fixture();
        _knownValues = Array.ConvertAll(RoadSegmentCategory.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentCategory.Unknown,
                RoadSegmentCategory.NotApplicable,
                RoadSegmentCategory.MainRoad,
                RoadSegmentCategory.LocalRoad,
                RoadSegmentCategory.LocalRoadType1,
                RoadSegmentCategory.LocalRoadType2,
                RoadSegmentCategory.LocalRoadType3,
                RoadSegmentCategory.PrimaryRoadI,
                RoadSegmentCategory.PrimaryRoadII,
                RoadSegmentCategory.PrimaryRoadIIType1,
                RoadSegmentCategory.PrimaryRoadIIType2,
                RoadSegmentCategory.PrimaryRoadIIType3,
                RoadSegmentCategory.PrimaryRoadIIType4,
                RoadSegmentCategory.SecondaryRoad,
                RoadSegmentCategory.SecondaryRoadType1,
                RoadSegmentCategory.SecondaryRoadType2,
                RoadSegmentCategory.SecondaryRoadType3,
                RoadSegmentCategory.SecondaryRoadType4,
                RoadSegmentCategory.EuropeanMainRoad,
                RoadSegmentCategory.FlemishMainRoad,
                RoadSegmentCategory.RegionalRoad,
                RoadSegmentCategory.InterLocalRoad,
                RoadSegmentCategory.LocalAccessRoad,
                RoadSegmentCategory.LocalHeritageAccessRoad
            },
            RoadSegmentCategory.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentCategory.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentCategory.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentCategory.CanParse(null));
    }

    [Fact]
    public void LocalRoadReturnsExpectedResult()
    {
        Assert.Equal("LocalRoad", RoadSegmentCategory.LocalRoad);
    }

    [Fact]
    public void LocalRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal("L", RoadSegmentCategory.LocalRoad.Translation.Identifier);
    }

    [Fact]
    public void LocalRoadType1ReturnsExpectedResult()
    {
        Assert.Equal("LocalRoadType1", RoadSegmentCategory.LocalRoadType1);
    }

    [Fact]
    public void LocalRoadType1TranslationReturnsExpectedResult()
    {
        Assert.Equal("L1", RoadSegmentCategory.LocalRoadType1.Translation.Identifier);
    }

    [Fact]
    public void LocalRoadType2ReturnsExpectedResult()
    {
        Assert.Equal("LocalRoadType2", RoadSegmentCategory.LocalRoadType2);
    }

    [Fact]
    public void LocalRoadType2TranslationReturnsExpectedResult()
    {
        Assert.Equal("L2", RoadSegmentCategory.LocalRoadType2.Translation.Identifier);
    }

    [Fact]
    public void LocalRoadType3ReturnsExpectedResult()
    {
        Assert.Equal("LocalRoadType3", RoadSegmentCategory.LocalRoadType3);
    }

    [Fact]
    public void LocalRoadType3TranslationReturnsExpectedResult()
    {
        Assert.Equal("L3", RoadSegmentCategory.LocalRoadType3.Translation.Identifier);
    }

    [Fact]
    public void MainRoadReturnsExpectedResult()
    {
        Assert.Equal("MainRoad", RoadSegmentCategory.MainRoad);
    }

    [Fact]
    public void MainRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal("H", RoadSegmentCategory.MainRoad.Translation.Identifier);
    }

    [Fact]
    public void NotApplicableReturnsExpectedResult()
    {
        Assert.Equal("NotApplicable", RoadSegmentCategory.NotApplicable);
    }

    [Fact]
    public void NotApplicableTranslationReturnsExpectedResult()
    {
        Assert.Equal("-9", RoadSegmentCategory.NotApplicable.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentCategory.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentCategory.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentCategory.Parse(null));
    }

    [Fact]
    public void PrimaryRoadIIReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadII", RoadSegmentCategory.PrimaryRoadII);
    }

    [Fact]
    public void PrimaryRoadIITranslationReturnsExpectedResult()
    {
        Assert.Equal("PII", RoadSegmentCategory.PrimaryRoadII.Translation.Identifier);
    }

    [Fact]
    public void PrimaryRoadIIType1ReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadIIType1", RoadSegmentCategory.PrimaryRoadIIType1);
    }

    [Fact]
    public void PrimaryRoadIIType1TranslationReturnsExpectedResult()
    {
        Assert.Equal("PII-1", RoadSegmentCategory.PrimaryRoadIIType1.Translation.Identifier);
    }

    [Fact]
    public void PrimaryRoadIIType2ReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadIIType2", RoadSegmentCategory.PrimaryRoadIIType2);
    }

    [Fact]
    public void PrimaryRoadIIType2TranslationReturnsExpectedResult()
    {
        Assert.Equal("PII-2", RoadSegmentCategory.PrimaryRoadIIType2.Translation.Identifier);
    }

    [Fact]
    public void PrimaryRoadIIType3ReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadIIType3", RoadSegmentCategory.PrimaryRoadIIType3);
    }

    [Fact]
    public void PrimaryRoadIIType3TranslationReturnsExpectedResult()
    {
        Assert.Equal("PII-3", RoadSegmentCategory.PrimaryRoadIIType3.Translation.Identifier);
    }

    [Fact]
    public void PrimaryRoadIIType4ReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadIIType4", RoadSegmentCategory.PrimaryRoadIIType4);
    }

    [Fact]
    public void PrimaryRoadIIType4TranslationReturnsExpectedResult()
    {
        Assert.Equal("PII-4", RoadSegmentCategory.PrimaryRoadIIType4.Translation.Identifier);
    }

    [Fact]
    public void PrimaryRoadIReturnsExpectedResult()
    {
        Assert.Equal("PrimaryRoadI", RoadSegmentCategory.PrimaryRoadI);
    }

    [Fact]
    public void PrimaryRoadITranslationReturnsExpectedResult()
    {
        Assert.Equal("PI", RoadSegmentCategory.PrimaryRoadI.Translation.Identifier);
    }

    [Fact]
    public void SecondaryRoadReturnsExpectedResult()
    {
        Assert.Equal("SecondaryRoad", RoadSegmentCategory.SecondaryRoad);
    }

    [Fact]
    public void SecondaryRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal("S", RoadSegmentCategory.SecondaryRoad.Translation.Identifier);
    }

    [Fact]
    public void SecondaryRoadType1ReturnsExpectedResult()
    {
        Assert.Equal("SecondaryRoadType1", RoadSegmentCategory.SecondaryRoadType1);
    }

    [Fact]
    public void SecondaryRoadType1TranslationReturnsExpectedResult()
    {
        Assert.Equal("S1", RoadSegmentCategory.SecondaryRoadType1.Translation.Identifier);
    }

    [Fact]
    public void SecondaryRoadType2ReturnsExpectedResult()
    {
        Assert.Equal("SecondaryRoadType2", RoadSegmentCategory.SecondaryRoadType2);
    }

    [Fact]
    public void SecondaryRoadType2TranslationReturnsExpectedResult()
    {
        Assert.Equal("S2", RoadSegmentCategory.SecondaryRoadType2.Translation.Identifier);
    }

    [Fact]
    public void SecondaryRoadType3ReturnsExpectedResult()
    {
        Assert.Equal("SecondaryRoadType3", RoadSegmentCategory.SecondaryRoadType3);
    }

    [Fact]
    public void SecondaryRoadType3TranslationReturnsExpectedResult()
    {
        Assert.Equal("S3", RoadSegmentCategory.SecondaryRoadType3.Translation.Identifier);
    }

    [Fact]
    public void SecondaryRoadType4ReturnsExpectedResult()
    {
        Assert.Equal("SecondaryRoadType4", RoadSegmentCategory.SecondaryRoadType4);
    }

    [Fact]
    public void SecondaryRoadType4TranslationReturnsExpectedResult()
    {
        Assert.Equal("S4", RoadSegmentCategory.SecondaryRoadType4.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentCategory.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentCategory.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentCategory.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentCategory.TryParse(null, out _));
    }

    [Fact]
    public void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", RoadSegmentCategory.Unknown);
    }

    [Fact]
    public void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal("-8", RoadSegmentCategory.Unknown.Translation.Identifier);
    }

    [Fact]
    public void IsUpgraded()
    {
        var upgraded = new[]
        {
            RoadSegmentCategory.EuropeanMainRoad,
            RoadSegmentCategory.FlemishMainRoad,
            RoadSegmentCategory.RegionalRoad,
            RoadSegmentCategory.InterLocalRoad,
            RoadSegmentCategory.LocalAccessRoad,
            RoadSegmentCategory.LocalHeritageAccessRoad
        };

        foreach (var category in RoadSegmentCategory.All)
        {
            var isUpgraded = RoadSegmentCategory.IsUpgraded(category);
            var expected = upgraded.Contains(category);

            Assert.Equal(expected, isUpgraded);
        }
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
        ).Verify(typeof(RoadSegmentCategory));
    }
}
