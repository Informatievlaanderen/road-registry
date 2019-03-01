namespace RoadRegistry.BackOffice.Translation
{
    using System;

    public readonly struct ArchiveId : IEquatable<ArchiveId>
    {
        private readonly string _value;

        public ArchiveId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "The archive identifier must not be null or empty.");

            _value = value;
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
