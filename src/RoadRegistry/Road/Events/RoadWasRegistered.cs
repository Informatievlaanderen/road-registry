namespace RoadRegistry.Road.Events
{
    using System;
    using Aiv.Vbr.EventHandling;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("RoadWasRegistered")]
    [EventDescription("De weg werd aangemaakt.")]
    public class RoadWasRegistered
    {
        public Guid RoadId { get; }

        public RoadWasRegistered(
            RoadId roadId)
        {
            RoadId = roadId;
        }

        [JsonConstructor]
        private RoadWasRegistered(
            Guid roadId)
            : this(
                  new RoadId(roadId))
        { }
    }
}
