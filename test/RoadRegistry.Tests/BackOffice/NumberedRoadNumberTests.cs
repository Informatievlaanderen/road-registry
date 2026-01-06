namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class NumberedRoadNumberTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public NumberedRoadNumberTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(NumberedRoadNumbers.All, type => type.ToString());
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueDoesNotEndWithDigits()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var not_a_digit_position = _fixture.Create<int>() % 6;
        var all_digits_except_for_one = new char[6];
        for (var index = 0; index < 6; index++)
        {
            if (index == not_a_digit_position)
            {
                all_digits_except_for_one[index] = generator.First(candidate => !char.IsDigit(candidate));
            }
            else
            {
                all_digits_except_for_one[index] = generator.First(char.IsDigit);
            }
        }

        var value =
            new string(new[] { roadType }.Concat(all_digits_except_for_one).Concat(new[] { '1' }).ToArray());

        var result = NumberedRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueDoesNotEndWithDigitThatIsOneOrTwo()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var all_digits = new char[6];
        for (var index = 0; index < 6; index++)
        {
            all_digits[index] = generator.First(char.IsDigit);
        }

        var not_one_nor_two = generator
            .First(candidate =>
                char.IsDigit(candidate) && candidate != '1' && candidate != '2');
        var value =
            new string(new[] { roadType }.Concat(all_digits).Concat(new[] { not_one_nor_two }).ToArray());

        var result = NumberedRoadNumber.CanParse(value);
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
                            !RoadTypes.All.Any(
                                letter => letter.Equals(candidate))),
                '1', '1', '1', '1', '1', '1', '1'
            });

        var result = NumberedRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsNot8CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length != 8);

        var result = NumberedRoadNumber.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = NumberedRoadNumber.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NumberedRoadNumber.CanParse(null));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.Equal(value, NumberedRoadNumber.Parse(value).ToString());
    }

    [Fact]
    public void ParseThrowsWhenValueDoesNotEndWithDigits()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var not_a_digit_position = _fixture.Create<int>() % 6;
        var all_digits_except_for_one = new char[6];
        for (var index = 0; index < 6; index++)
        {
            if (index == not_a_digit_position)
            {
                all_digits_except_for_one[index] = generator.First(candidate => !char.IsDigit(candidate));
            }
            else
            {
                all_digits_except_for_one[index] = generator.First(char.IsDigit);
            }
        }

        var value =
            new string(new[] { roadType }.Concat(all_digits_except_for_one).Concat(new[] { '1' }).ToArray());

        Assert.Throws<FormatException>(() => NumberedRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueDoesNotEndWithDigitThatIsOneOrTwo()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var all_digits = new char[6];
        for (var index = 0; index < 6; index++)
        {
            all_digits[index] = generator.First(char.IsDigit);
        }

        var not_one_nor_two = generator
            .First(candidate =>
                char.IsDigit(candidate) && candidate != '1' && candidate != '2');
        var value =
            new string(new[] { roadType }.Concat(all_digits).Concat(new[] { not_one_nor_two }).ToArray());

        Assert.Throws<FormatException>(() => NumberedRoadNumber.Parse(value));
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
                '1', '1', '1', '1', '1', '1', '1'
            });

        Assert.Throws<FormatException>(() => NumberedRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseThrowsWhenValueIsNot8CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length != 8);

        Assert.Throws<FormatException>(() => NumberedRoadNumber.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NumberedRoadNumber.Parse(null));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = NumberedRoadNumber.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueDoesNotEndWithDigits()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var not_a_digit_position = _fixture.Create<int>() % 6;
        var all_digits_except_for_one = new char[6];
        for (var index = 0; index < 6; index++)
        {
            if (index == not_a_digit_position)
            {
                all_digits_except_for_one[index] = generator.First(candidate => !char.IsDigit(candidate));
            }
            else
            {
                all_digits_except_for_one[index] = generator.First(char.IsDigit);
            }
        }

        var value =
            new string(new[] { roadType }.Concat(all_digits_except_for_one).Concat(new[] { '1' }).ToArray());

        var result = NumberedRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueDoesNotEndWithDigitThatIsOneOrTwo()
    {
        var generator = new Generator<char>(_fixture);
        var roadType = generator
            .First(
                candidate =>
                    RoadTypes.All.Any(
                        letter => letter.Equals(candidate)));
        var all_digits = new char[6];
        for (var index = 0; index < 6; index++)
        {
            all_digits[index] = generator.First(char.IsDigit);
        }

        var not_one_nor_two = generator
            .First(candidate =>
                char.IsDigit(candidate) && candidate != '1' && candidate != '2');
        var value =
            new string(new[] { roadType }.Concat(all_digits).Concat(new[] { not_one_nor_two }).ToArray());

        var result = NumberedRoadNumber.TryParse(value, out var parsed);
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
                            !RoadTypes.All.Any(
                                letter => letter.Equals(candidate))),
                '1', '1', '1', '1', '1', '1', '1'
            });

        var result = NumberedRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsNot8CharactersLong()
    {
        var value =
            new Generator<string>(_fixture)
                .First(candidate => candidate.Length != 8);

        var result = NumberedRoadNumber.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellFormed()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = NumberedRoadNumber.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => NumberedRoadNumber.TryParse(null, out _));
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
        ).Verify(typeof(NumberedRoadNumber));
    }
}
