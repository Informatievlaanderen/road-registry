namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

[DataContract(Name = "StraatnaamOntkoppelen", Namespace = "")]
public sealed record UnlinkStreetNameFromRoadSegmentResponse : EndpointResponse
{
}

public class UnlinkStreetNameFromRoadSegmentResponseExamples : IExamplesProvider<UnlinkStreetNameFromRoadSegmentResponse>
{
    public UnlinkStreetNameFromRoadSegmentResponse GetExamples()
    {
        return new UnlinkStreetNameFromRoadSegmentResponse();
    }
}
