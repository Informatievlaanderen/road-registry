namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct UploadId : IEquatable<UploadId>
    {
        private readonly Guid _value;

        public UploadId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentNullException(nameof(value), "The upload identifier must not be empty.");

            _value = value;
        }

        public static bool Accepts(Guid value)
        {
            return value != Guid.Empty;
        }

        public bool Equals(UploadId other) => _value == other._value;
        public override bool Equals(object other) => other is UploadId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString("N");
        public Guid ToGuid() => _value;
        public static bool operator ==(UploadId left, UploadId right) => left.Equals(right);
        public static bool operator !=(UploadId left, UploadId right) => !left.Equals(right);
        public static implicit operator Guid(UploadId instance) => instance.ToGuid();
    }
}
