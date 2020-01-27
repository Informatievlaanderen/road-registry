namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct OrganizationName : IEquatable<OrganizationName>
    {
        public const int MaxLength = 64;

        private readonly string _value;

        public OrganizationName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value), "The organization name must not be null or empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The organization name must be {MaxLength} characters or less.");
            }

            _value = value;
        }

        public static bool AcceptsValue(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
        }

        public bool Equals(OrganizationName other) => _value == other._value;
        public override bool Equals(object other) => other is OrganizationName id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(OrganizationName instance) => instance._value;
        public static bool operator ==(OrganizationName left, OrganizationName right) => left.Equals(right);
        public static bool operator !=(OrganizationName left, OrganizationName right) => !left.Equals(right);
    }
}
