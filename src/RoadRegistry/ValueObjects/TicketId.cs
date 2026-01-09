namespace RoadRegistry.ValueObjects;

using System;

public readonly struct TicketId : IEquatable<TicketId>
{
    private readonly Guid _value;

    public TicketId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(value), "The ticket identifier must not be empty.");
        }

        _value = value;
    }

    public static TicketId? FromValue(Guid? value)
    {
        if (value is null || value == Guid.Empty)
        {
            return null;
        }

        return new TicketId(value.Value);
    }

    public bool Equals(TicketId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is TicketId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static bool operator ==(TicketId left, TicketId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TicketId left, TicketId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator string?(TicketId? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator Guid(TicketId instance)
    {
        return instance._value;
    }
}
