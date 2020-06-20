namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct ChangeRequestId : IEquatable<ChangeRequestId>
    {
        private readonly Guid _value;

        public ChangeRequestId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentNullException(nameof(value), "The change request identifier must not be empty.");

            _value = value;
        }

        public static bool Accepts(Guid value)
        {
            return value != Guid.Empty;
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
