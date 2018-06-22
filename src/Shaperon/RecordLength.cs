using System;

namespace Shaperon
{
    public readonly struct RecordLength : IEquatable<RecordLength>, IComparable<RecordLength>
    {
        private static readonly RecordLength Initial = new RecordLength(1);

        private readonly int _value;

        public RecordLength(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value of record length must be greater than or equal to 0.");
            }
            _value = value;
        }

        public RecordLength Plus(DbaseFieldLength other)
        {
            return new RecordLength(_value + other.ToInt32());
        }

        public RecordLength Plus(RecordLength other)
        {
            return new RecordLength(_value + other.ToInt32());
        }

        public int ToInt32() => _value;
        public ByteLength ToByteLength() => new ByteLength(_value * 2);
        public bool Equals(RecordLength instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is RecordLength length && Equals(length);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();

        public int CompareTo(RecordLength other)
        {
            return _value.CompareTo(other.ToInt32());
        }

        public static implicit operator int(RecordLength instance) => instance.ToInt32();

        public static RecordLength operator +(RecordLength left, RecordLength right) => left.Plus(right);
        public static bool operator ==(RecordLength left, RecordLength right) => left.Equals(right);
        public static bool operator !=(RecordLength left, RecordLength right) => !left.Equals(right);
        public static bool operator <(RecordLength left, RecordLength right) => left.CompareTo(right) < 0;
        public static bool operator <=(RecordLength left, RecordLength right) => left.CompareTo(right) <= 0;
        public static bool operator >=(RecordLength left, RecordLength right) => left.CompareTo(right) >= 0;
        public static bool operator >(RecordLength left, RecordLength right) => left.CompareTo(right) > 0;
    }
}
