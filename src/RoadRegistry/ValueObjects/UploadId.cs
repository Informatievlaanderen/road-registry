namespace RoadRegistry.ValueObjects;

using System;

public readonly struct UploadId : IEquatable<UploadId>
{
    private readonly Guid _value;

    public UploadId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentNullException(nameof(value), "The upload identifier must not be empty.");

        _value = value;
    }

    public static bool Accepts(Guid value)
    {
        return value != Guid.Empty;
    }

    public bool Equals(UploadId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is UploadId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString("N");
    }

    public Guid ToGuid()
    {
        return _value;
    }

    public static bool operator ==(UploadId left, UploadId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UploadId left, UploadId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Guid(UploadId instance)
    {
        return instance.ToGuid();
    }

    public static implicit operator string(UploadId instance)
    {
        return instance.ToString();
    }
}
