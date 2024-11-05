namespace RoadRegistry.SyncHost.Tests.Municipality.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Microsoft.EntityFrameworkCore;
using Sync.MunicipalityRegistry;

public class MunicipalityEventProjectionClassFixture : ConnectedProjectionFixture<MunicipalityEventProjection, MunicipalityEventConsumerContext>
{
    public MunicipalityEventProjectionClassFixture(IDbContextFactory<MunicipalityEventConsumerContext> dbContextFactory)
        : base(dbContextFactory.CreateDbContext(),
            Resolve.WhenEqualToHandlerMessageType(new MunicipalityEventProjection().Handlers))
    {
    }
}
