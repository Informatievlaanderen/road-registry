namespace RoadRegistry.BackOffice.Core
{
    using RoadRegistry.RoadNetwork.ValueObjects;

    public class NextAttributeIdProvider
    {
        private AttributeId _nextValue;

        public NextAttributeIdProvider(AttributeId initialValue)
        {
            _nextValue = initialValue;
        }

        public AttributeId Next()
        {
            var result = _nextValue;
            _nextValue = _nextValue.Next();
            return result;
        }
    }
}
