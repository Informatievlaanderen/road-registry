﻿using System;

namespace Shaperon
{
    public readonly struct DbaseRecordLength : IEquatable<DbaseRecordLength>, IComparable<DbaseRecordLength>
    {
        public static readonly DbaseRecordLength Initial = new DbaseRecordLength(1);

        private readonly int _value;

        public DbaseRecordLength(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value of the dbase record length must be greater than or equal to 0.");
            }
            _value = value;
        }

        public DbaseRecordLength Plus(DbaseFieldLength other)
        {
            return new DbaseRecordLength(_value + other.ToInt32());
        }

        public DbaseRecordLength Plus(DbaseRecordLength other)
        {
            return new DbaseRecordLength(_value + other.ToInt32());
        }

        public int ToInt32() => _value;
        public bool Equals(DbaseRecordLength instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is DbaseRecordLength length && Equals(length);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();

        public int CompareTo(DbaseRecordLength other)
        {
            return _value.CompareTo(other.ToInt32());
        }

        public static implicit operator int(DbaseRecordLength instance) => instance.ToInt32();

        public static DbaseRecordLength operator +(DbaseRecordLength left, DbaseFieldLength right) => left.Plus(right);
        public static DbaseRecordLength operator +(DbaseRecordLength left, DbaseRecordLength right) => left.Plus(right);
        public static bool operator ==(DbaseRecordLength left, DbaseRecordLength right) => left.Equals(right);
        public static bool operator !=(DbaseRecordLength left, DbaseRecordLength right) => !left.Equals(right);
        public static bool operator <(DbaseRecordLength left, DbaseRecordLength right) => left.CompareTo(right) < 0;
        public static bool operator <=(DbaseRecordLength left, DbaseRecordLength right) => left.CompareTo(right) <= 0;
        public static bool operator >=(DbaseRecordLength left, DbaseRecordLength right) => left.CompareTo(right) >= 0;
        public static bool operator >(DbaseRecordLength left, DbaseRecordLength right) => left.CompareTo(right) > 0;
    }
}
