namespace RoadRegistry.ValueObjects;

using System;

public readonly struct ArchiveId : IEquatable<ArchiveId>
{
    public const int MaxLength = 32;
    private readonly string _value;

    public ArchiveId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), "The archive identifier must not be null or empty.");

        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The archive identifier must be {MaxLength} characters or less.");

        _value = value;
    }

    public static bool Accepts(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
    }

    public static ArchiveId FromGuid(Guid value)
    {
        return new ArchiveId(value.ToString("N"));
    }

    public bool Equals(ArchiveId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is ArchiveId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public Guid ToGuid()
    {
        return Guid.Parse(_value);
    }

    public static bool operator ==(ArchiveId left, ArchiveId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArchiveId left, ArchiveId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator string(ArchiveId instance)
    {
        return instance._value;
    }

    public static implicit operator Guid(ArchiveId instance)
    {
        return instance.ToGuid();
    }
}
