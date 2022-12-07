namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

[DataContract(Name = "StraatnaamOntkoppelen", Namespace = "")]
public sealed record UnlinkStreetNameResponse : EndpointResponse
{
}

public class UnlinkStreetNameResponseExamples : IExamplesProvider<UnlinkStreetNameResponse>
{
    public UnlinkStreetNameResponse GetExamples()
    {
        return new UnlinkStreetNameResponse();
    }
}
