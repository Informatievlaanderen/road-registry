namespace RoadRegistry.BackOffice;

using System;
using Extensions;
using Framework;

public readonly struct MunicipalityNisCode
{
    private readonly string _value;

    public MunicipalityNisCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!AcceptsValue(value))
        {
            throw new ArgumentException("The value is not a well known nis-code", nameof(value));
        }

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.ContainsWhitespace() && value.Length == 5;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(MunicipalityNisCode instance)
    {
        return instance.ToString();
    }
}
