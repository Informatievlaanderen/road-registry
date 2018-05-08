using System;

namespace Shaperon
{
    public readonly struct ByteLength : IEquatable<ByteLength>
    {
        private readonly int _value;

        public ByteLength(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value", "The value of byte length must be greater than or equal to 0.");
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

        public int ToInt32() => _value;
        public WordLength ToWordLength() 
        {
            if(_value % 2 != 0) 
            {
                throw new InvalidOperationException("The byte length needs to be divisible by 2.");
            }
            return new WordLength(_value / 2);
        }
        public bool Equals(ByteLength instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is ByteLength && Equals((ByteLength)obj);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static implicit operator int(ByteLength instance) => instance.ToInt32();
    }
}