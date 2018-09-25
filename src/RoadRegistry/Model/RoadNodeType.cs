namespace RoadRegistry.Model
{
    using System;

    public class RoadNodeType : IEquatable<RoadNodeType>
    {
        private readonly int _value;

        public static readonly RoadNodeType RealNode = new RoadNodeType(1);
        public static readonly RoadNodeType FakeNode = new RoadNodeType(2);
        public static readonly RoadNodeType EndNode = new RoadNodeType(3);
        public static readonly RoadNodeType MiniRoundabout = new RoadNodeType(4);
        public static readonly RoadNodeType TurnLoopNode = new RoadNodeType(5);

        public static readonly RoadNodeType[] All = {RealNode, FakeNode, EndNode, MiniRoundabout, TurnLoopNode};

        private RoadNodeType(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadNodeType parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadNodeType Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road node type.");
            }
            return parsed;
        }

        public bool Equals(RoadNodeType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadNodeType type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadNodeType instance) => instance.ToInt32();
        public static bool operator ==(RoadNodeType left, RoadNodeType right) => Equals(left, right);
        public static bool operator !=(RoadNodeType left, RoadNodeType right) => !Equals(left, right);
    }
}
