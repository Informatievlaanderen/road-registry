namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadNodeTypeTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadNodeTypeTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(RoadNodeType.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadNodeType.RealNode,
                RoadNodeType.FakeNode,
                RoadNodeType.EndNode,
                RoadNodeType.MiniRoundabout,
                RoadNodeType.TurningLoopNode
            },
            RoadNodeType.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadNodeType.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadNodeType.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadNodeType.CanParse(null));
    }

    [Fact]
    public void EndNodeReturnsExpectedResult()
    {
        Assert.Equal("EndNode", RoadNodeType.EndNode);
    }

    [Fact]
    public void EndNodeTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RoadNodeType.EndNode.Translation.Identifier);
    }

    [Fact]
    public void FakeNodeReturnsExpectedResult()
    {
        Assert.Equal("FakeNode", RoadNodeType.FakeNode);
    }

    [Fact]
    public void FakeNodeTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RoadNodeType.FakeNode.Translation.Identifier);
    }

    [Fact]
    public void IsAnyOfReturnsExpectedResultWhenNoneOf()
    {
        _fixture.CustomizeRoadNodeType();

        var generator = new Generator<RoadNodeType>(_fixture);
        var sut = _fixture.Create<RoadNodeType>();
        var noneOf = generator.Where(candidate => !candidate.Equals(sut)).Take(2).ToArray();

        Assert.False(sut.IsAnyOf(noneOf));
    }

    [Fact]
    public void IsAnyOfReturnsExpectedResultWhenOneOf()
    {
        _fixture.CustomizeRoadNodeType();

        var generator = new Generator<RoadNodeType>(_fixture);
        var sut = _fixture.Create<RoadNodeType>();
        var oneOf = generator.Where(candidate => !candidate.Equals(sut)).Take(2).Concat(new[] { sut }).ToArray();

        Assert.True(sut.IsAnyOf(oneOf));
    }

    [Fact]
    public void IsAnyOfReturnsThrowsWhenTypesIsNull()
    {
        _fixture.CustomizeRoadNodeType();

        var sut = _fixture.Create<RoadNodeType>();
        Assert.Throws<ArgumentNullException>(() => sut.IsAnyOf(null));
    }

    [Fact]
    public void MiniRoundaboutReturnsExpectedResult()
    {
        Assert.Equal("MiniRoundabout", RoadNodeType.MiniRoundabout);
    }

    [Fact]
    public void MiniRoundaboutTranslationReturnsExpectedResult()
    {
        Assert.Equal(4, RoadNodeType.MiniRoundabout.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadNodeType.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadNodeType.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadNodeType.Parse(null));
    }

    [Fact]
    public void RealNodeReturnsExpectedResult()
    {
        Assert.Equal("RealNode", RoadNodeType.RealNode);
    }

    [Fact]
    public void RealNodeTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RoadNodeType.RealNode.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadNodeType.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadNodeType.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadNodeType.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadNodeType.TryParse(null, out _));
    }

    [Fact]
    public void TurningLoopNodeReturnsExpectedResult()
    {
        Assert.Equal("TurningLoopNode", RoadNodeType.TurningLoopNode);
    }

    [Fact]
    public void TurningLoopNodeTranslationReturnsExpectedResult()
    {
        Assert.Equal(5, RoadNodeType.TurningLoopNode.Translation.Identifier);
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
        ).Verify(typeof(RoadNodeType));
    }
}
