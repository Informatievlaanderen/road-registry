using System;

namespace Shaperon
{
    public struct DbaseDecimalCount : IEquatable<DbaseDecimalCount>
    {
        private readonly int _value;

        public DbaseDecimalCount(int value)
        {
            if(value < 0 || value > 254)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The decimal count of a dbase field must be between 0 and 254.");
            _value = value;
        }

        public bool Equals(DbaseDecimalCount other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is DbaseDecimalCount count && Equals(count);
        public override int GetHashCode() => _value;
        public int ToInt32() => _value;
        public byte ToByte() => Convert.ToByte(_value);
        public override string ToString() => _value.ToString();
        public static implicit operator int(DbaseDecimalCount instance) => instance.ToInt32();
        public static implicit operator byte(DbaseDecimalCount instance) => instance.ToByte();
    }
}
