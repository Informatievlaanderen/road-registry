namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct DownloadId : IEquatable<DownloadId>
    {
        private readonly Guid _value;

        public DownloadId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentNullException(nameof(value), "The download identifier must not be empty.");

            _value = value;
        }

        public static bool Accepts(Guid value)
        {
            return value != Guid.Empty;
        }

        public static bool CanParse(string value)
        {
            return Guid.TryParseExact(value, "N", out var guid) && Accepts(guid);
        }

        public static DownloadId Parse(string value)
        {
            return new DownloadId(Guid.ParseExact(value, "N"));
        }

        public bool Equals(DownloadId other) => _value == other._value;
        public override bool Equals(object other) => other is DownloadId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString("N");
        public Guid ToGuid() => _value;
        public static bool operator ==(DownloadId left, DownloadId right) => left.Equals(right);
        public static bool operator !=(DownloadId left, DownloadId right) => !left.Equals(right);
        public static implicit operator Guid(DownloadId instance) => instance.ToGuid();
    }
}
