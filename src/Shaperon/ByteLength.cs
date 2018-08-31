using System;

namespace Shaperon
{
    using System.Linq;

    public readonly struct ByteLength : IEquatable<ByteLength>
    {
        private readonly int _value;

        public ByteLength(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value of byte length must be greater than or equal to 0.");
            }
            if(value % 2 != 0)
            {
                throw new ArgumentException("The value of byte length must be a multiple of 2 (even).", nameof(value));
            }
            _value = value;
        }

        public ByteLength Plus(ByteLength other)
        {
            return new ByteLength(_value + other.ToInt32());
        }

        public ByteLength Plus(WordLength other)
        {
            return new ByteLength(_value + other.ToByteLength().ToInt32());
        }

        public ByteLength Times(int times)
        {
            return new ByteLength(_value * times);
        }

        public int ToInt32() => _value;
        public WordLength ToWordLength()
        {
            return new WordLength(_value / 2);
        }
        public bool Equals(ByteLength instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is ByteLength length && Equals(length);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static implicit operator int(ByteLength instance) => instance.ToInt32();

        public static ByteLength Int32(string valueName) => Int32s(valueName);
        public static ByteLength Int32s(params string[] valueNames) => valueNames.Aggregate(new ByteLength(0), (length, name) => length.Plus(new ByteLength(4)));
        public static ByteLength Double(string valueName) => Doubles(valueName);
        public static ByteLength Doubles(params string[] valueNames) => valueNames.Aggregate(new ByteLength(0), (length, name) => length.Plus(new ByteLength(8)));
    }
}
