namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RoadSegmentWidthAttributes : IEnumerable<RoadSegmentWidthAttribute>
    {
        private readonly IReadOnlyCollection<RoadSegmentWidthAttribute> _attributes;

        public RoadSegmentWidthAttributes(IReadOnlyCollection<RoadSegmentWidthAttribute> attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }
            var position = new RoadSegmentPosition(0);
            foreach(var attribute in attributes)
            {
                if(attribute.From != position)
                {
                    throw new ArgumentException("The width attribute does not properly align as the first attribute or with its previous attribute.");
                }
                position = attribute.To;
            }
            _attributes = attributes;
        }
        
        public IEnumerator<RoadSegmentWidthAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}