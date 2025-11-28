namespace RoadRegistry.ValueObjects;

using System;
using System.Linq;

public struct NumberedRoadNumber : IEquatable<NumberedRoadNumber>, IComparable<NumberedRoadNumber>
{
    private readonly char[] _value;

    private NumberedRoadNumber(char[] value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return value.Length == 8
               && RoadTypes.All.Any(candidate => candidate.Equals(value[0]))
               && Enumerable.Range(1, 7).All(position => char.IsDigit(value[position]))
               && (value[7] == '1' || value[7] == '2');
    }

    public static bool TryParse(string value, out NumberedRoadNumber parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value.Length != 8)
        {
            parsed = default;
            return false;
        }

        if (!RoadTypes.All.Any(candidate => candidate.Equals(value[0])))
        {
            parsed = default;
            return false;
        }

        if (!Enumerable.Range(1, 7).All(position => char.IsDigit(value[position])))
        {
            parsed = default;
            return false;
        }

        if (value[7] != '1' && value[7] != '2')
        {
            parsed = default;
            return false;
        }

        parsed = new NumberedRoadNumber(value.ToCharArray());
        return true;
    }

    public static NumberedRoadNumber Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value.Length != 8) throw new FormatException($"The numbered road number value must be exactly 8 characters long. Actual value was {value}.");

        if (!RoadTypes.All.Any(candidate => candidate.Equals(value[0]))) throw new FormatException($"The numbered road number value must have a road type of {string.Join(',', RoadTypes.All)}. Actual value was {value}.");

        if (!Enumerable.Range(1, 7).All(position => char.IsDigit(value[position]))) throw new FormatException($"The numbered road number value must end with digits. Actual value was {value}.");

        if (value[7] != '1' && value[7] != '2') throw new FormatException($"The numbered road number value must end with a digit that is either 1 or 2. Actual value was {value}.");

        return new NumberedRoadNumber(value.ToCharArray());
    }

    public int CompareTo(NumberedRoadNumber other)
    {
        return string.CompareOrdinal(ToString(), other.ToString());
    }

    public override bool Equals(object obj)
    {
        return obj is NumberedRoadNumber type && Equals(type);
    }

    public bool Equals(NumberedRoadNumber other)
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

    public static implicit operator string(NumberedRoadNumber instance)
    {
        return instance.ToString();
    }

    public static bool operator ==(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NumberedRoadNumber left, NumberedRoadNumber right)
    {
        return left.CompareTo(right) >= 0;
    }
}
