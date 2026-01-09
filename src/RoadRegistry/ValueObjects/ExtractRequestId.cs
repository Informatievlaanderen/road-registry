namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public readonly struct ExtractRequestId : IEquatable<ExtractRequestId>
{
    public const int ExactLength = 32;
    public const int ExactStringLength = 64;
    private readonly byte[] _value;

    public ExtractRequestId(byte[] value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value.Length != ExactLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The extract request identifier must be {ExactLength} bytes.");

        _value = value;
    }

    public static bool Accepts(string value)
    {
        return !string.IsNullOrEmpty(value)
               && value.Length == ExactStringLength
               && Enumerable.Range(0, ExactStringLength / 2).All(index =>
                   byte.TryParse(new ReadOnlySpan<char>(new[] { value[index * 2], value[index * 2 + 1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
               );
    }

    public static ExtractRequestId FromString(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value),
                "The extract request identifier must not be null or empty.");

        if (value.Length != ExactStringLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The extract request identifier must be {ExactStringLength} characters.");

        if (Enumerable.Range(0, ExactStringLength / 2).Any(index =>
                !byte.TryParse(new ReadOnlySpan<char>(new[] { value[index * 2], value[index * 2 + 1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
            ))
            throw new ArgumentException(
                "The extract request identifier must consist of hexadecimal characters only.", nameof(value));
        return new ExtractRequestId(Enumerable
            .Range(0, ExactStringLength / 2)
            .Select(index =>
                byte.Parse(new ReadOnlySpan<char>(new[] { value[index * 2], value[index * 2 + 1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
            .ToArray());
    }

    public static ExtractRequestId FromExternalRequestId(ExternalExtractRequestId id)
    {
        using (var hash = SHA256.Create())
        {
            return new ExtractRequestId(
                hash.ComputeHash(
                    Encoding.UTF8.GetBytes(id.ToString())
                )
            );
        }
    }

    public IReadOnlyCollection<byte> ToBytes()
    {
        return _value;
    }

    public bool Equals(ExtractRequestId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is ExtractRequestId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return string.Concat(_value.Select(value => value.ToString("X2"))).ToLowerInvariant();
    }

    public static bool operator ==(ExtractRequestId left, ExtractRequestId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ExtractRequestId left, ExtractRequestId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator string?(ExtractRequestId? instance)
    {
        return instance?.ToString();
    }
}
