namespace RoadRegistry.Model
{
    using System;

    public readonly struct MaintainerId : IEquatable<MaintainerId>
    {
        private readonly string _value;

        public MaintainerId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "The maintainer identifier must not be null or empty.");

            _value = value;
        }

        public bool Equals(MaintainerId other) => _value == other._value;
        public override bool Equals(object other) => other is MaintainerId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(MaintainerId instance) => instance._value;
        public static bool operator ==(MaintainerId left, MaintainerId right) => left.Equals(right);
        public static bool operator !=(MaintainerId left, MaintainerId right) => !left.Equals(right);
    }
}
