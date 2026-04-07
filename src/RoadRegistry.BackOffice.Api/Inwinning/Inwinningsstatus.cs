namespace RoadRegistry.BackOffice.Api.Inwinning;

using System.Collections.Generic;

public sealed class Inwinningsstatus : IDutchToString
{
    public static Inwinningsstatus NietGestart => new("nietGestart");
    public static Inwinningsstatus Locked => new("locked");
    public static Inwinningsstatus Compleet => new("compleet");

    public static readonly IReadOnlyCollection<Inwinningsstatus> All = [NietGestart, Locked, Compleet];

    private readonly string _value;

    private Inwinningsstatus(string value)
    {
        _value = value;
    }

    public string ToDutchString()
    {
        return _value;
    }

    public static implicit operator string (Inwinningsstatus instance) => instance._value;
}
