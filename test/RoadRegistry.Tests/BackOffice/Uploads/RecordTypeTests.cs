namespace RoadRegistry.Tests.BackOffice.Uploads;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;

public class RecordTypeTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RecordTypeTests()
    {
        _fixture = new Fixture();
        _knownValues = Array.ConvertAll(RecordType.All, type => type.ToString());
    }

    [Fact]
    public void AddedReturnsExpectedResult()
    {
        Assert.Equal("Added", RecordType.Added);
    }

    [Fact]
    public void AddedTranslationReturnsExpectedResult()
    {
        Assert.Equal(2, RecordType.Added.Translation.Identifier);
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RecordType.Identical,
                RecordType.Added,
                RecordType.Modified,
                RecordType.Removed
            },
            RecordType.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RecordType.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RecordType.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RecordType.CanParse(null));
    }

    [Fact]
    public void EqualTranslationReturnsExpectedResult()
    {
        Assert.Equal(1, RecordType.Identical.Translation.Identifier);
    }

    [Fact]
    public void IdenticalReturnsExpectedResult()
    {
        Assert.Equal("Identical", RecordType.Identical);
    }

    [Fact]
    public void IsAnyOfReturnsExpectedResultIfItIsAnyOfThese()
    {
        _fixture.CustomizeRecordType();
        var these = _fixture.CreateMany<RecordType>(3).Distinct().ToArray();
        var sut = these[new Random().Next(0, these.Length)];

        Assert.True(sut.IsAnyOf(these));
    }

    [Fact]
    public void IsAnyOfReturnsExpectedResultIfItIsNotAnyOfThese()
    {
        _fixture.CustomizeRecordType();
        var these = _fixture.CreateMany<RecordType>(3).Distinct().ToArray();
        var sut = new Generator<RecordType>(_fixture).First(candidate => Array.IndexOf(these, candidate) == -1);

        Assert.False(sut.IsAnyOf(these));
    }

    [Fact]
    public void ModifiedReturnsExpectedResult()
    {
        Assert.Equal("Modified", RecordType.Modified);
    }

    [Fact]
    public void ModifiedTranslationReturnsExpectedResult()
    {
        Assert.Equal(3, RecordType.Modified.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RecordType.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RecordType.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RecordType.Parse(null));
    }

    [Fact]
    public void RemovedReturnsExpectedResult()
    {
        Assert.Equal("Removed", RecordType.Removed);
    }

    [Fact]
    public void RemovedTranslationReturnsExpectedResult()
    {
        Assert.Equal(4, RecordType.Removed.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RecordType.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RecordType.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RecordType.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RecordType.TryParse(null, out _));
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
        ).Verify(typeof(RecordType));
    }
}
