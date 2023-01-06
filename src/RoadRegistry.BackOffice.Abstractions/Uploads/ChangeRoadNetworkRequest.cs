namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using Messages;

public sealed record ChangeRoadNetworkRequest : EndpointRequest<ChangeRoadNetworkResponse>
{
    public RequestedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
}
