namespace RoadRegistry.Model
{
    using Framework;

    public readonly struct AttributeHash
    {
        public static readonly AttributeHash None = new AttributeHash(HashCode.Initial);

        private readonly HashCode _hashCode;

        private AttributeHash(HashCode hashCode)
        {
            _hashCode = hashCode;
        }

        public AttributeHash CombineWith(RoadSegmentAccessRestriction value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash CombineWith(RoadSegmentCategory value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash CombineWith(RoadSegmentMorphology value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash CombineWith(RoadSegmentStatus value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash CombineWith(CrabStreetnameId? value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }

        public AttributeHash CombineWith(MaintenanceAuthorityId value)
        {
            return new AttributeHash(_hashCode.CombineWith(value));
        }
    }
}
