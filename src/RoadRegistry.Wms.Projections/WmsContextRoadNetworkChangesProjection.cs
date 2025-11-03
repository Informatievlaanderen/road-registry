namespace RoadRegistry.Wms.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Wms.Schema;

public class WmsContextRoadNetworkChangesProjection : DbContextRoadNetworkChangesProjection<WmsContext>
{
    public WmsContextRoadNetworkChangesProjection(ConnectedProjectionHandlerResolver<WmsContext> resolver, IDbContextFactory<WmsContext> dbContextFactory)
        : base(resolver, dbContextFactory)
    {
    }
}
