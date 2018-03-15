namespace Wegenregister.Projections.Oslo
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Road;
    using RoadList;

    public class RoadOsloRunner : Runner<OsloContext>
    {
        public RoadOsloRunner(EnvelopeFactory envelopeFactory) :
            base(
                "RoadOsloRunner",
                envelopeFactory,
                new RoadListProjections(),
                new RoadProjections()) { }
    }
}
