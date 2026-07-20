namespace RoadRegistry.Projector.Infrastructure.Options;

public class ProjectionOptions
{
    public Option Product { get; set; }
    public Option Editor { get; set; }
    public Option Wms { get; set; }
    public Option Wfs { get; set; }
    public Option ProducerSnapshot { get; set; }
    public Option BackOfficeProcessors { get; set; }
    public Option OrganizationSync { get; set; }
    public Option StreetNameSync { get; set; }
    public Option Integration { get; set; }
    public Option MartenMigration { get; set; }
    public Option Read { get; set; }
    public Option Extract { get; set; }
    public Option Pbs { get; set; }
    public Option WmsWfsV2 { get; set; }

    public class Option
    {
        public bool Enabled { get; set; }
    }
}
