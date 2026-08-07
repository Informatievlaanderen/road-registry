namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;

// How far the inwinning of a road segment or a municipality has got: not started, running, or done.
public sealed class Inwinningsstatus : IEquatable<Inwinningsstatus>, IDutchToString
{
    public static readonly Inwinningsstatus NietGestart = new("nietGestart");
    public static readonly Inwinningsstatus Locked = new("locked");
    public static readonly Inwinningsstatus Compleet = new("compleet");

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

    public override string ToString()
    {
        return _value;
    }

    public bool Equals(Inwinningsstatus? other)
    {
        return other is not null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Inwinningsstatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(Inwinningsstatus? left, Inwinningsstatus? right) => Equals(left, right);
    public static bool operator !=(Inwinningsstatus? left, Inwinningsstatus? right) => !Equals(left, right);

    public static implicit operator string (Inwinningsstatus instance) => instance._value;
}
