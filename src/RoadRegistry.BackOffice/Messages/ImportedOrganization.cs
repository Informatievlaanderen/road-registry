namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("ImportedOrganization")]
    [EventDescription("Indicates an organization was imported.")]
    public class ImportedOrganization
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
