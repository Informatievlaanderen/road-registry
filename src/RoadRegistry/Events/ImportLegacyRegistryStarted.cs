namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;
    using System;

    [EventName("ImportLegacyRegistryStarted")]
    [EventDescription("Indicates the import of the legacy registry was started.")]
    public class ImportLegacyRegistryStarted
    {
        public DateTime StartedAt { get; set; }
    }
}
