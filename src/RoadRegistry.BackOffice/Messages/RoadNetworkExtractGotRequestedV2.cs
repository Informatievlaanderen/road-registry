namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;
using RoadRegistry.BackOffice.Core;

[EventName("RoadNetworkExtractGotRequestedV2")]
[EventDescription("Indicates a road network extract was requested.")]
public class RoadNetworkExtractGotRequestedV2 : IRoadNetworkExtractGotRequestedMessage, IWhen
{
    public RoadNetworkExtractGeometry Contour { get; set; }
    public string Description { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public bool IsInformative { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
    public string When { get; set; }
}
