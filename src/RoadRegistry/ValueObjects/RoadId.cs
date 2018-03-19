namespace RoadRegistry.ValueObjects
{
    using System;
    using Aiv.Vbr.AggregateSource;
    using Newtonsoft.Json;

    public class RoadId : GuidValueObject<RoadId>
    {
        public RoadId([JsonProperty("value")] Guid roadId) : base(roadId) { }
    }
}
