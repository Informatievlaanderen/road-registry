namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct ExternalExtractRequestId : IEquatable<ExternalExtractRequestId>
    {
        public const int MaxLength = 256;

        private readonly string _value;

        public ExternalExtractRequestId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value), "The external extract request identifier must not be null or empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The external extract request identifier must be {MaxLength} characters or less.");
            }

            _value = value;
        }

        public static bool AcceptsValue(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
        }

        public bool Equals(ExternalExtractRequestId other) => _value == other._value;
        public override bool Equals(object other) => other is ExternalExtractRequestId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(ExternalExtractRequestId instance) => instance.ToString();
        public static bool operator ==(ExternalExtractRequestId left, ExternalExtractRequestId right) => left.Equals(right);
        public static bool operator !=(ExternalExtractRequestId left, ExternalExtractRequestId right) => !left.Equals(right);
    }
}
