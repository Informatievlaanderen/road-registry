namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractGotRequestedV2")]
[EventDescription("Indicates a road network extract was requested.")]
public class RoadNetworkExtractGotRequestedV2 : IRoadNetworkExtractGotRequestedMessage
{
    public RoadNetworkExtractGeometry Contour { get; set; }
    public string Description { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string When { get; set; }
}

public interface IRoadNetworkExtractGotRequestedMessage : IMessage
{
    public RoadNetworkExtractGeometry Contour { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string When { get; set; }
}
