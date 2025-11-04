namespace RoadRegistry.Wms.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Infrastructure.MartenDb.Projections;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Wms.Schema;

public class WmsContextRoadNetworkChangesProjection : RoadNetworkChangesRunnerDbContextProjection<WmsContext>
{
    public WmsContextRoadNetworkChangesProjection(ConnectedProjectionHandlerResolver<WmsContext> resolver, IDbContextFactory<WmsContext> dbContextFactory)
        : base(resolver, dbContextFactory)
    {
    }
}
