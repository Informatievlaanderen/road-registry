namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class NationalRoadNumberTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public NationalRoadNumberTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(NationalRoadNumbers.All, type => type.ToString());
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueDoesNotHaveDigitAsSecondCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate))
            });

        var result = NationalRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueDoesNotStartWithAcceptableRoadType()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            !RoadTypes.All.Any(letter => letter.Equals(candidate))),
                '2'
            });

        var result = NationalRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsNotBetween2And5CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length < 2 || candidate.Length > 5);
        var result = NationalRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = NationalRoadNumber.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueLongerThan2CharactersDoesNotHaveDigitOrLetterAsLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate) && !char.IsLetter(candidate)),
                1
            );

        var result = NationalRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueLongerThan3CharactersDoesNotHaveDigitFromTheThirdUntilTheSecondLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate)),
                new Random().Next(1, 3)
            )
            + new string(new Generator<char>(_fixture).First(char.IsLetter), 1);

        var result = NationalRoadNumber.CanParse(value);
        Assert.False(result);
    }

    // --

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NationalRoadNumber.CanParse(null));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.Equal(value, NationalRoadNumber.Parse(value).ToString());
    }

    [Fact]
    public void ParseThrowsWhenValueDoesNotHaveDigitAsSecondCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate))
            });

        Assert.Throws<FormatException>(() => NationalRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueDoesNotStartWithAcceptableRoadType()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            !RoadTypes.All.Any(letter => letter.Equals(candidate))),
                '2'
            });

        Assert.Throws<FormatException>(() => NationalRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueIsNotBetween2And5CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length < 2 || candidate.Length > 5);

        Assert.Throws<FormatException>(() => NationalRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueLongerThan2CharactersDoesNotHaveDigitOrLetterAsLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate) && !char.IsLetter(candidate)),
                1
            );

        Assert.Throws<FormatException>(() => NationalRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueLongerThan3CharactersDoesNotHaveDigitFromTheThirdUntilTheSecondLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate)),
                new Random().Next(1, 3)
            )
            + new string(new Generator<char>(_fixture).First(char.IsLetter), 1);

        Assert.Throws<FormatException>(() => NationalRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NationalRoadNumber.Parse(null));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = NationalRoadNumber.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueDoesNotHaveDigitAsSecondCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate))
            });

        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueDoesNotStartWithAcceptableRoadType()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            !RoadTypes.All.Any(letter => letter.Equals(candidate))),
                '2'
            });

        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsNotBetween2And5CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length < 2 || candidate.Length > 5);
        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueLongerThan2CharactersDoesNotHaveDigitOrLetterAsLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate) && !char.IsLetter(candidate)),
                1
            );

        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueLongerThan3CharactersDoesNotHaveDigitFromTheThirdUntilTheSecondLastCharacter()
    {
        var value =
            new string(new[]
            {
                new Generator<char>(_fixture)
                    .First(
                        candidate =>
                            RoadTypes.All.Any(letter => letter.Equals(candidate))),
                new Generator<char>(_fixture)
                    .First(char.IsDigit)
            })
            + new string(new Generator<char>(_fixture)
                    .First(candidate => !char.IsDigit(candidate)),
                new Random().Next(1, 3)
            )
            + new string(new Generator<char>(_fixture).First(char.IsLetter), 1);

        var result = NationalRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NationalRoadNumber.TryParse(null, out _));
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
        ).Verify(typeof(NationalRoadNumber));
    }
}
