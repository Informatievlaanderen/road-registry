namespace RoadRegistry.Model
{
    using System;

    public readonly struct MaintenanceAuthorityName : IEquatable<MaintenanceAuthorityName>
    {
        public const int MaxLength = 64;
        
        private readonly string _value;

        public MaintenanceAuthorityName(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "The maintainer identifier must not be null or empty.");
            if (value.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The maintainer identifier must be {MaxLength} characters or less.");

            _value = value;
        }

        public bool Equals(MaintenanceAuthorityName other) => _value == other._value;
        public override bool Equals(object other) => other is MaintenanceAuthorityName id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(MaintenanceAuthorityName instance) => instance._value;
        public static bool operator ==(MaintenanceAuthorityName left, MaintenanceAuthorityName right) => left.Equals(right);
        public static bool operator !=(MaintenanceAuthorityName left, MaintenanceAuthorityName right) => !left.Equals(right);
    }
}