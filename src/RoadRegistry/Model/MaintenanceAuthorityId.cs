namespace RoadRegistry.Model
{
    using System;

    public readonly struct MaintenanceAuthorityId : IEquatable<MaintenanceAuthorityId>
    {
        private readonly string _value;

        public MaintenanceAuthorityId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "The maintainer identifier must not be null or empty.");

            _value = value;
        }

        public bool Equals(MaintenanceAuthorityId other) => _value == other._value;
        public override bool Equals(object other) => other is MaintenanceAuthorityId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(MaintenanceAuthorityId instance) => instance._value;
        public static bool operator ==(MaintenanceAuthorityId left, MaintenanceAuthorityId right) => left.Equals(right);
        public static bool operator !=(MaintenanceAuthorityId left, MaintenanceAuthorityId right) => !left.Equals(right);
    }
}
