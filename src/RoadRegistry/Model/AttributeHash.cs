namespace RoadRegistry.Model
{
    using System;
    using System.Diagnostics.Contracts;
    using Framework;

    public readonly struct AttributeHash : IEquatable<AttributeHash>
    {
        public static readonly AttributeHash None = new AttributeHash(HashCode.Initial);

        private readonly HashCode _hashCode;

        private AttributeHash(HashCode hashCode)
        {
            _hashCode = hashCode;
        }

        public AttributeHash With(RoadSegmentAccessRestriction value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash With(RoadSegmentCategory value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash With(RoadSegmentMorphology value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash With(RoadSegmentStatus value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash WithLeft(CrabStreetnameId? value)
        {
            return new AttributeHash(_hashCode.CombineWith(value).CombineWith('L'));
        }

        public AttributeHash WithRight(CrabStreetnameId? value)
        {
            return new AttributeHash(_hashCode.CombineWith(value).CombineWith('R'));
        }

        public AttributeHash With(MaintenanceAuthorityId value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        [Pure]
        public bool Equals(AttributeHash other) => _hashCode.Equals(other._hashCode);
        public override bool Equals(object obj) => obj is AttributeHash other && Equals(other);
        public override int GetHashCode() => _hashCode;
    }
}
