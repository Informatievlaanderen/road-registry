namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractClosed")]
[EventDescription("Indicates a road network extract has been closed and no further upload is expected or allowed.")]
public class RoadNetworkExtractClosed : IMessage, IWhen
{
    public string RequestId { get; set; }
    public string ExternalRequestId { get; set; }
    public string[] DownloadIds { get; set; }
    public string When { get; set; }
    public DateTime DateRequested { get; set; }
    public RoadNetworkExtractCloseReason Reason { get; set; }
}
