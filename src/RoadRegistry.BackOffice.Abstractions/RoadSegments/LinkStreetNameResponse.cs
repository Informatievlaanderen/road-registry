namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

[DataContract(Name = "StraatnaamKoppelen", Namespace = "")]
public sealed record LinkStreetNameResponse : EndpointResponse
{
}

public class LinkStreetNameResponseExamples : IExamplesProvider<LinkStreetNameResponse>
{
    public LinkStreetNameResponse GetExamples()
    {
        return new LinkStreetNameResponse();
    }
}
