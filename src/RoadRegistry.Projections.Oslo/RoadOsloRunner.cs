namespace RoadRegistry.Projections.Oslo
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;
    using Road;
    using RoadList;

    public class RoadOsloRunner : Runner<OsloContext>
    {
        public RoadOsloRunner(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory) :
            base(
                "RoadOsloRunner",
                envelopeFactory,
                loggerFactory.CreateLogger("RoadRegistryOsloRunner"),
                new RoadListProjections(),
                new RoadProjections()) { }
    }
}
