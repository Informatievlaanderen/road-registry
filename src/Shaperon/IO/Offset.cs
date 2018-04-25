using System;

namespace Shaperon.IO
{
    public readonly struct Offset : IEquatable<Offset>, IComparable<Offset>
    {
        public static readonly Offset Initial = new Offset(50); //100 byte file header means first record appears at offset 50 (16-bit word) of the mainfile.
        
        private readonly int _value;

        public Offset(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value", "The value of word length must be greater than or equal to 0.");
            }
            _value = value;
        }

        public Offset Plus(WordLength other) 
        {
            return new Offset(_value + other.ToInt32());
        }

        public Offset Plus(ByteLength other) 
        {
            return new Offset(_value + other.ToWordLength().ToInt32());
        }

        public int ToInt32() => _value;
        public ByteLength ToByteLength() => new ByteLength(_value * 2);
        public bool Equals(Offset instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is WordLength && Equals((WordLength)obj);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();

        public int CompareTo(Offset other)
        {
            return _value.CompareTo(other.ToInt32());
        }

        public static implicit operator int(Offset instance) => instance.ToInt32();
        public static Offset operator +(Offset left, Offset right) => new Offset(left.ToInt32() + right.ToInt32());
        public static bool operator ==(Offset left, Offset right) => left.Equals(right);
        public static bool operator !=(Offset left, Offset right) => !left.Equals(right);
        public static bool operator <(Offset left, Offset right) => left.CompareTo(right) < 0;
        public static bool operator <=(Offset left, Offset right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Offset left, Offset right) => left.CompareTo(right) >= 0;
        public static bool operator >(Offset left, Offset right) => left.CompareTo(right) > 0;
    }
}