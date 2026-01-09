namespace RoadRegistry.ValueObjects;

using System;
using System.Linq;

public readonly struct NationalRoadNumber : IEquatable<NationalRoadNumber>, IComparable<NationalRoadNumber>
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 5;
    private readonly char[] _value;

    private NationalRoadNumber(char[] value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        // Must adhere to the length constraints
        if (!(value.Length >= MinimumLength
              && value.Length <= MaximumLength))
            return false;

        // First character needs to be a road type
        if (!RoadTypes.All.Any(candidate => candidate.Equals(value[0]))) return false;

        // Second character needs to be a digit
        if (!char.IsDigit(value[1])) return false;

        if (value.Length > 2)
        {
            // Last character needs to be a letter or a digit
            if (!(char.IsDigit(value[value.Length - 1]) || char.IsLetter(value[value.Length - 1]))) return false;

            if (value.Length > 3
                && !Enumerable.Range(2, value.Length - 3)
                    .All(position => char.IsDigit(value[position])))
                // From the third character onwards until the second last character must be all digits
                return false;
        }

        return true;
    }

    public static bool TryParse(string value, out NationalRoadNumber parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        // Must adhere to the length constraints
        if (!(value.Length >= MinimumLength
              && value.Length <= MaximumLength))
        {
            parsed = default;
            return false;
        }

        // First character needs to be a road type
        if (!RoadTypes.All.Any(candidate => candidate.Equals(value[0])))
        {
            parsed = default;
            return false;
        }

        // Second character needs to be a digit that differs from 0
        if (!char.IsDigit(value[1]))
        {
            parsed = default;
            return false;
        }

        if (value.Length > 2)
        {
            // Last character needs to be a letter or a digit
            if (!(char.IsDigit(value[value.Length - 1]) || char.IsLetter(value[value.Length - 1])))
            {
                parsed = default;
                return false;
            }

            if (value.Length > 3
                && !Enumerable.Range(2, value.Length - 3)
                    .All(position => char.IsDigit(value[position])))
            {
                // From the third character onwards until the second last character must be all digits
                parsed = default;
                return false;
            }
        }

        parsed = new NationalRoadNumber(value.ToCharArray());
        return true;
    }

    public static NationalRoadNumber Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        // Must adhere to the length constraints
        if (!(value.Length >= MinimumLength
              && value.Length <= MaximumLength))
            throw new FormatException($"The national road number value must be between {MinimumLength} and {MaximumLength} characters long. Actual value was {value}.");

        // First character needs to be a road type
        if (!RoadTypes.All.Any(candidate => candidate.Equals(value[0]))) throw new FormatException($"The national road number value must have a road type of {string.Join(',', RoadTypes.All)}. Actual value was {value}.");

        // Second character needs to be a digit
        if (!char.IsDigit(value[1])) throw new FormatException($"The national road number value must have a digit as its second character. Actual value was {value}.");

        if (value.Length > 2)
        {
            // Last character needs to be a letter or a digit
            if (!(char.IsDigit(value[value.Length - 1]) || char.IsLetter(value[value.Length - 1]))) throw new FormatException($"The national road number value must end with a digit or a letter. Actual value was {value}.");

            if (value.Length > 3
                && !Enumerable.Range(2, value.Length - 3)
                    .All(position => char.IsDigit(value[position])))
                // From the third character onwards until the second last character must be all digits
                throw new FormatException($"The national road number value must be all digits as of the third character until the second last character. Actual value was {value}.");
        }

        return new NationalRoadNumber(value.ToCharArray());
    }

    public int CompareTo(NationalRoadNumber other)
    {
        return string.CompareOrdinal(ToString(), other.ToString());
    }

    public override bool Equals(object obj)
    {
        return obj is NationalRoadNumber type && Equals(type);
    }

    public bool Equals(NationalRoadNumber other)
    {
        return (other._value ?? []).SequenceEqual(_value ?? []);
    }

    public override int GetHashCode()
    {
        return new string(_value ?? []).GetHashCode();
    }

    public override string ToString()
    {
        return new string(_value ?? []);
    }

    public static implicit operator string?(NationalRoadNumber? instance)
    {
        return instance?.ToString();
    }

    public static bool operator ==(NationalRoadNumber left, NationalRoadNumber right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(NationalRoadNumber left, NationalRoadNumber right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(NationalRoadNumber left, NationalRoadNumber right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NationalRoadNumber left, NationalRoadNumber right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(NationalRoadNumber left, NationalRoadNumber right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NationalRoadNumber left, NationalRoadNumber right)
    {
        return left.CompareTo(right) >= 0;
    }
}
