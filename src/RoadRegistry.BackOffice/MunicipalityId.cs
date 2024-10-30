namespace RoadRegistry.BackOffice;

using System;
using Extensions;

public readonly struct MunicipalityId
{
    private readonly string _value;

    public MunicipalityId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!AcceptsValue(value))
        {
            throw new ArgumentException("The value is not a well known municipality identifier", nameof(value));
        }

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.ContainsWhitespace();
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(MunicipalityId instance)
    {
        return instance.ToString();
    }
}
