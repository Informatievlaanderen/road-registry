using System;

namespace Shaperon
{
    public readonly struct WordOffset : IEquatable<WordOffset>, IComparable<WordOffset>
    {
        private readonly int _value;

        public WordOffset(int value)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value of word offset must be greater than or equal to 0.");
            }
            _value = value;
        }

        public WordOffset Plus(WordLength other)
        {
            return new WordOffset(_value + other.ToInt32());
        }

        public WordOffset Plus(ByteLength other)
        {
            return new WordOffset(_value + other.ToWordLength().ToInt32());
        }

        public int ToInt32() => _value;
        public ByteLength ToByteLength() => new ByteLength(_value * 2);
        public bool Equals(WordOffset instance) => instance._value == _value;
        public override bool Equals(object obj) => obj is WordLength length && Equals(length);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();

        public int CompareTo(WordOffset other)
        {
            return _value.CompareTo(other.ToInt32());
        }

        public static implicit operator int(WordOffset instance) => instance.ToInt32();
        public static WordOffset operator +(WordOffset left, WordOffset right) => new WordOffset(left.ToInt32() + right.ToInt32());
        public static bool operator ==(WordOffset left, WordOffset right) => left.Equals(right);
        public static bool operator !=(WordOffset left, WordOffset right) => !left.Equals(right);
        public static bool operator <(WordOffset left, WordOffset right) => left.CompareTo(right) < 0;
        public static bool operator <=(WordOffset left, WordOffset right) => left.CompareTo(right) <= 0;
        public static bool operator >=(WordOffset left, WordOffset right) => left.CompareTo(right) >= 0;
        public static bool operator >(WordOffset left, WordOffset right) => left.CompareTo(right) > 0;
    }
}
