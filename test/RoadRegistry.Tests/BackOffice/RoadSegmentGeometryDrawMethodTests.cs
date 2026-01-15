namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentGeometryDrawMethodTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentGeometryDrawMethodTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(RoadSegmentGeometryDrawMethod.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentGeometryDrawMethod.Outlined,
                RoadSegmentGeometryDrawMethod.Measured,
                RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications
            },
            RoadSegmentGeometryDrawMethod.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentGeometryDrawMethod.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentGeometryDrawMethod.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentGeometryDrawMethod.CanParse(null));
    }

    [Fact]
    public void Measured_according_to_GRB_specificationsReturnsExpectedResult()
    {
        Assert.Equal("Measured_according_to_GRB_specifications", RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications);
    }

    [Fact]
    public void Measured_according_to_GRB_specificationsTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications.Translation.Identifier);
    }

    [Fact]
    public void MeasuredReturnsExpectedResult()
    {
        Assert.Equal("Measured", RoadSegmentGeometryDrawMethod.Measured);
    }

    [Fact]
    public void MeasuredTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier);
    }

    [Fact]
    public void OutlinedReturnsExpectedResult()
    {
        Assert.Equal("Outlined", RoadSegmentGeometryDrawMethod.Outlined);
    }

    [Fact]
    public void OutlinedTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentGeometryDrawMethod.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentGeometryDrawMethod.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentGeometryDrawMethod.Parse(null));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentGeometryDrawMethod.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentGeometryDrawMethod.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentGeometryDrawMethod.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentGeometryDrawMethod.TryParse(null, out _));
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
        ).Verify(typeof(RoadSegmentGeometryDrawMethod));
    }
}
