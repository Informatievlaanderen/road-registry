namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractGotRequested")]
[EventDescription("Indicates a road network extract was requested.")]
public class RoadNetworkExtractGotRequested : IMessage
{
    public string RequestId { get; set; }
    public string ExternalRequestId { get; set; }
    public Guid DownloadId { get; set; }
    public RoadNetworkExtractGeometry Contour { get; set; }
    public string When { get; set; }
}
