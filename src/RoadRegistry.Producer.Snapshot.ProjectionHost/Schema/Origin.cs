namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Schema
{
    using System;

    public class Origin
    {
        public DateTimeOffset? BeginTime { get; set; }
        public string Organization { get; set; }
    }
}
