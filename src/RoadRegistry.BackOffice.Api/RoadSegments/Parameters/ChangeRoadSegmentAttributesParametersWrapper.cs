namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Collections.Generic;

public class ChangeRoadSegmentAttributesParametersWrapper
{
    public List<ChangeAttributeParameters> Attributes { get; set; }

    public static implicit operator ChangeRoadSegmentAttributesParameters(ChangeRoadSegmentAttributesParametersWrapper p) => p.Attributes as ChangeRoadSegmentAttributesParameters;
    public static explicit operator ChangeRoadSegmentAttributesParametersWrapper(ChangeRoadSegmentAttributesParameters p) => new() { Attributes = p };
}