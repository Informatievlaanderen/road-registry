namespace RoadRegistry.Model
{
    using System;

    public class LaneDirection : IEquatable<LaneDirection>
    {
        private readonly int _value;

        public static readonly LaneDirection Unknown = new LaneDirection(-8);
        public static readonly LaneDirection Forward = new LaneDirection(1);
        public static readonly LaneDirection Backward = new LaneDirection(2);
        public static readonly LaneDirection Independent = new LaneDirection(3);

        public static readonly LaneDirection[] All = {
            Unknown, Forward, Backward, Independent
        };

        private LaneDirection(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out LaneDirection parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static LaneDirection Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known lane direction.");
            }
            return parsed;
        }

        public bool Equals(LaneDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is LaneDirection type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(LaneDirection instance) => instance.ToInt32();
        public static bool operator ==(LaneDirection left, LaneDirection right) => Equals(left, right);
        public static bool operator !=(LaneDirection left, LaneDirection right) => !Equals(left, right);
    }
}
