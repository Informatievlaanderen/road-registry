namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;
    using System;

    [EventName("ImportLegacyRegistryFinished")]
    [EventDescription("Indicates the import of the legacy registry was finished.")]
    public class ImportLegacyRegistryFinished
    {
        public DateTime FinishedAt { get; set; }
    }
}
