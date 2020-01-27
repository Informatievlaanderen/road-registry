namespace RoadRegistry.BackOffice
{
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

        public bool Equals(ArchiveId other) => _value == other._value;
        public override bool Equals(object other) => other is ArchiveId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static bool operator ==(ArchiveId left, ArchiveId right) => left.Equals(right);
        public static bool operator !=(ArchiveId left, ArchiveId right) => !left.Equals(right);
        public static implicit operator string(ArchiveId instance) => instance._value;
    }
}
