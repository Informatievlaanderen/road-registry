namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using System;

    public record JobResponse(
        Guid Id,
        Uri? TicketUrl,
        JobStatus Status,
        DateTimeOffset Created,
        DateTimeOffset LastChanged
    );
}
