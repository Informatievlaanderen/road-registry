namespace RoadRegistry.ValueObjects;

using System;

public readonly struct DownloadId : IEquatable<DownloadId>
{
    private readonly Guid _value;

    public DownloadId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentNullException(nameof(value), "The download identifier must not be empty.");

        _value = value;
    }

    public static bool Accepts(Guid value)
    {
        return value != Guid.Empty;
    }

    public static DownloadId? FromValue(Guid? value)
    {
        return value.HasValue
            ? new DownloadId(value.Value)
            : null;
    }

    public static bool CanParse(string value)
    {
        return TryParse(value, out _);
    }

    public static DownloadId Parse(string value)
    {
        return new DownloadId(Guid.ParseExact(value, "N"));
    }
    public static bool TryParse(string value, out DownloadId downloadId)
    {
        if (Guid.TryParseExact(value, "N", out var guid) && Accepts(guid))
        {
            downloadId = new DownloadId(guid);
            return true;
        }

        downloadId = default;
        return false;
    }

    public bool Equals(DownloadId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is DownloadId id && Equals(id);
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

    public static bool operator ==(DownloadId left, DownloadId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DownloadId left, DownloadId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Guid(DownloadId instance)
    {
        return instance.ToGuid();
    }

    public static implicit operator string(DownloadId instance)
    {
        return instance.ToString();
    }
}
