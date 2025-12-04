namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections;
using System.Collections.Generic;

public class RoadSegmentSurfaceAttributes : IEnumerable<RoadSegmentSurfaceAttribute>
{
    private readonly IReadOnlyCollection<RoadSegmentSurfaceAttribute> _attributes;

    public RoadSegmentSurfaceAttributes(IReadOnlyCollection<RoadSegmentSurfaceAttribute> attributes)
    {
        if (attributes == null) throw new ArgumentNullException(nameof(attributes));
        var position = new RoadSegmentPosition(0);
        var index = 0;
        foreach (var attribute in attributes)
        {
            if (attribute.From != position)
                throw new ArgumentException(
                    $"The road segment surface attributes are not adjacent. The attribute at index {index} was expected to start from {position} but actually starts from {attribute.From}.",
                    nameof(attributes));
            position = attribute.To;
            index++;
        }

        _attributes = attributes;
    }

    public IEnumerator<RoadSegmentSurfaceAttribute> GetEnumerator()
    {
        return _attributes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
