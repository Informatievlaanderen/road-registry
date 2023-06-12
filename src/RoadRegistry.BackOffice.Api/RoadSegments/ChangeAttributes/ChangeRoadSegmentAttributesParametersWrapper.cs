namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System.Collections.Generic;

public class ChangeRoadSegmentAttributesParametersWrapper
{
    public List<ChangeAttributeParameters> Attributes { get; set; }

    public static explicit operator ChangeRoadSegmentAttributesParametersWrapper(ChangeRoadSegmentAttributesParameters p)
    {
        return new ChangeRoadSegmentAttributesParametersWrapper { Attributes = p };
    }

    public static implicit operator ChangeRoadSegmentAttributesParameters(ChangeRoadSegmentAttributesParametersWrapper p)
    {
        return p.Attributes as ChangeRoadSegmentAttributesParameters;
    }
}
