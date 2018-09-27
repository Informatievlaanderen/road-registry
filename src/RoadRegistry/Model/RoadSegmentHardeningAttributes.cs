namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RoadSegmentHardeningAttributes : IEnumerable<RoadSegmentHardeningAttribute>
    {
        private readonly IReadOnlyCollection<RoadSegmentHardeningAttribute> _attributes;

        public RoadSegmentHardeningAttributes(IReadOnlyCollection<RoadSegmentHardeningAttribute> attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }
            var position = new RoadSegmentPosition(0);
            var index = 0;
            foreach(var attribute in attributes)
            {
                if(attribute.From != position)
                {
                    throw new ArgumentException(
                        $"The road segment hardening attributes are not adjacent. The attribute at index {index} was expected to start from {position} but actually starts from {attribute.From}.", 
                        nameof(attributes));
                }
                position = attribute.To;
                index++;
            }
            _attributes = attributes;
        }
        
        public IEnumerator<RoadSegmentHardeningAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}