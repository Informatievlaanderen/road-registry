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
    public ShadowedOption Pbs { get; set; }
    public ShadowedOption WmsWfsV2 { get; set; }

    public class Option
    {
        public bool Enabled { get; set; }
    }

    // A read model that can be rebuilt beside itself: a shadow copy of the projection fills a schema of its own from
    // the start of the event stream while the live one keeps serving, and is swapped in once it has caught up.
    public class ShadowedOption : Option
    {
        public ShadowOption Shadow { get; set; } = new();
    }

    public class ShadowOption
    {
        // Runs the shadow projection next to the live one.
        public bool Enabled { get; set; }

        // Makes the shadow read model the live one, before the projections start. Only done while the shadow
        // projection is switched off and its tables are there with a position no further behind than the live one;
        // otherwise it does nothing, so it can be left on after the swap.
        public bool Promote { get; set; }
    }
}
