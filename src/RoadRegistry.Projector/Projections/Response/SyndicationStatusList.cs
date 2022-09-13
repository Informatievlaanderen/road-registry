namespace RoadRegistry.Projector.Projections.Response
{
    using System.Collections.Generic;

    public class SyndicationStatusList
    {
        public IList<SyndicationStatus> Syndications { get; set; }
    }

    public class SyndicationStatus
    {
        public string Name { get; set; }

        public long CurrentPosition { get; set; }
    }
}
