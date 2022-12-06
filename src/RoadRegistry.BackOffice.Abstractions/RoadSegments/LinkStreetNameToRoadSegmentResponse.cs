namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

[DataContract(Name = "StraatnaamKoppelen", Namespace = "")]
public sealed record LinkStreetNameToRoadSegmentResponse : EndpointResponse
{
}

public class LinkStreetNameToRoadSegmentResponseExamples : IExamplesProvider<LinkStreetNameToRoadSegmentResponse>
{
    public LinkStreetNameToRoadSegmentResponse GetExamples()
    {
        return new LinkStreetNameToRoadSegmentResponse();
    }
}
