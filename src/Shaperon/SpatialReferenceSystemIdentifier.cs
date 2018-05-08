namespace Shaperon
{
    using System;

    public readonly struct SpatialReferenceSystemIdentifier : IEquatable<SpatialReferenceSystemIdentifier>
    {
        public static readonly SpatialReferenceSystemIdentifier BelgeLambert1972 = 
            new SpatialReferenceSystemIdentifier(103300);
            
        private readonly int _value;

        public SpatialReferenceSystemIdentifier(int value)
        {
            _value = value;
        }

        public bool Equals(SpatialReferenceSystemIdentifier other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is SpatialReferenceSystemIdentifier && Equals((SpatialReferenceSystemIdentifier)obj);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;
        public static implicit operator Int32(SpatialReferenceSystemIdentifier instance) => instance.ToInt32();
    }
}
