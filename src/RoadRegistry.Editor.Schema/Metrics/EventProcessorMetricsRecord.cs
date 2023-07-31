namespace RoadRegistry.Editor.Schema.Metrics;

public class EventProcessorMetricsRecord
{
    public string EventProcessorId { get; set; }
    public long FromPosition { get; set; }
    public long ToPosition { get; set; }
    public long ElapsedMilliseconds { get; set; }
}
