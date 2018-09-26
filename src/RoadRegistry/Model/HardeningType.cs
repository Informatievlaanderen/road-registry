namespace RoadRegistry.Model
{
    using System;

    public class HardeningType : IEquatable<HardeningType>
    {
        private readonly int _value;

        public static readonly HardeningType NotApplicable = new HardeningType(-9);
        public static readonly HardeningType Unknown = new HardeningType(-8);
        public static readonly HardeningType SolidHardening = new HardeningType(1);
        public static readonly HardeningType LooseHardening = new HardeningType(2);

        public static readonly HardeningType[] All = {
            NotApplicable, Unknown, SolidHardening, LooseHardening
        };

        private HardeningType(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out HardeningType parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static HardeningType Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road hardening.");
            }
            return parsed;
        }

        public bool Equals(HardeningType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is HardeningType type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(HardeningType instance) => instance.ToInt32();
        public static bool operator ==(HardeningType left, HardeningType right) => Equals(left, right);
        public static bool operator !=(HardeningType left, HardeningType right) => !Equals(left, right);
    }
}
