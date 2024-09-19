namespace RoadRegistry.BackOffice.Messages;

using System;

public class CheckCommandHostHealth
{
    public Guid TicketId { get; set; }
    public string FileName { get; set; }
}
