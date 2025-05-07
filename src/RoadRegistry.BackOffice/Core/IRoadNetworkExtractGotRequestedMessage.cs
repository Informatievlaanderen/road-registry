namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;
using RoadRegistry.BackOffice.Messages;

public interface IRoadNetworkExtractGotRequestedMessage : IMessage
{
    RoadNetworkExtractGeometry Contour { get; set; }
    Guid DownloadId { get; set; }
    string ExternalRequestId { get; set; }
    string RequestId { get; set; }
    bool IsInformative { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
}
