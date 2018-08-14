using System;

namespace Shaperon
{
    public readonly struct DbaseFieldLength : IEquatable<DbaseFieldLength>
    {
        public static readonly DbaseFieldLength MinLength = new DbaseFieldLength(0);
        public static readonly DbaseFieldLength MaxLength = new DbaseFieldLength(254);

        private readonly int _value;

        public DbaseFieldLength(int value)
        {
            if (value < 0 || value > 254)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The length of a dbase field must be between 0 and 254.");
            _value = value;
        }

        public bool Equals(DbaseFieldLength other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is DbaseFieldLength length && Equals(length);
        public override int GetHashCode() => _value;
        public int ToInt32() => _value;
        public byte ToByte() => Convert.ToByte(_value);
        public override string ToString() => _value.ToString();
        public static implicit operator int(DbaseFieldLength instance) => instance.ToInt32();
        public static implicit operator byte(DbaseFieldLength instance) => instance.ToByte();
    }
}
