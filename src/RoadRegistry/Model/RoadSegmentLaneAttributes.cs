namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RoadSegmentLaneAttributes : IEnumerable<RoadSegmentLaneAttribute>
    {
        private readonly IReadOnlyCollection<RoadSegmentLaneAttribute> _attributes;

        public RoadSegmentLaneAttributes(IReadOnlyCollection<RoadSegmentLaneAttribute> attributes)
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
                    throw new ArgumentException("The lane attribute does not properly align as the first attribute or with its previous attribute.");
                }
                position = attribute.To;
            }
            _attributes = attributes;
        }
        
        public IEnumerator<RoadSegmentLaneAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}