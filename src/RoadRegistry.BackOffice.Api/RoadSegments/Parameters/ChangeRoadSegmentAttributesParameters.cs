namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentAttribuutWijzigen", Namespace = "")]
public class ChangeRoadSegmentAttributesParameters : List<ChangeAttributeParameters>
{
}
