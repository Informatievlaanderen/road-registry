namespace RoadRegistry.BackOffice.Metrics;

using System;

public class EventProcessorMetricsRecord
{
    public Guid Id { get; set; }
    public string EventProcessorId { get; set; }
    public string DbContext { get; set; }
    public long FromPosition { get; set; }
    public long ToPosition { get; set; }
    public long ElapsedMilliseconds { get; set; }
}
