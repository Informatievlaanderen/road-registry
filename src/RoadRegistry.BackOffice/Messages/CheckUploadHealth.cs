namespace RoadRegistry.BackOffice.Messages;

using System;

public class CheckUploadHealth
{
    public Guid TicketId { get; set; }
    public string FileName { get; set; }
}
