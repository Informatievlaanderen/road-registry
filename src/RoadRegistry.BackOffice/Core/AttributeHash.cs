namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Diagnostics.Contracts;

    public readonly struct AttributeHash : IEquatable<AttributeHash>
    {
        public static readonly AttributeHash None = new AttributeHash(Framework.HashCode.Initial);

        public static AttributeHash FromHashCode(int value) => new AttributeHash(Framework.HashCode.FromHashCode(value));

        private readonly Framework.HashCode _hashCode;

        private AttributeHash(Framework.HashCode hashCode)
        {
            _hashCode = hashCode;
        }

        [Pure]
        public AttributeHash With(RoadSegmentAccessRestriction value)
        {
            return new AttributeHash(_hashCode.Hash(value));
        }

        [Pure]
        public AttributeHash With(RoadSegmentCategory value)
        {
            return new AttributeHash(_hashCode.Hash(value));
        }

        [Pure]
        public AttributeHash With(RoadSegmentMorphology value)
        {
            return new AttributeHash(_hashCode.Hash(value));
        }

        [Pure]
        public AttributeHash With(RoadSegmentStatus value)
        {
            return new AttributeHash(_hashCode.Hash(value));
        }

        [Pure]
        public AttributeHash WithLeftSide(CrabStreetnameId? value)
        {
            return new AttributeHash(_hashCode.Hash(value).Hash('L'));
        }

        [Pure]
        public AttributeHash WithRightSide(CrabStreetnameId? value)
        {
            return new AttributeHash(_hashCode.Hash(value).Hash('R'));
        }

        [Pure]
        public AttributeHash With(OrganizationId value)
        {
            return new AttributeHash(_hashCode.Hash(value));
        }

        [Pure]
        public bool Equals(AttributeHash other) => _hashCode.Equals(other._hashCode);
        public override bool Equals(object obj) => obj is AttributeHash other && Equals(other);
        public override int GetHashCode() => _hashCode;
        public override string ToString() => _hashCode.ToString();
        public static bool operator ==(AttributeHash left, AttributeHash right) => left.Equals(right);
        public static bool operator !=(AttributeHash left, AttributeHash right) => !left.Equals(right);
    }
}
