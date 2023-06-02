namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

public interface IRoadNetworkExtractClosedMessage : IMessage
{
    public DateTime When { get; set; }
    public Guid DownloadId { get; set; }
    public DateTime DateRequested { get; set; }
}
