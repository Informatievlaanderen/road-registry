namespace RoadRegistry.SyncHost.Tests.StreetName.Projections.Fixtures;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using RoadRegistry.Sync.StreetNameRegistry.Models;
using Sync.StreetNameRegistry;

public class StreetNameEventProjectionFixture : ConnectedProjectionFixture<StreetNameEventProjection, StreetNameEventProjectionContext>
{
    private readonly StreetNameEventProjectionContext _dbContext;

    public StreetNameEventProjectionFixture(StreetNameEventProjectionContext context)
        : base(context, Resolve.WhenEqualToHandlerMessageType(new StreetNameEventProjection().Handlers))
    {
        _dbContext = context;
    }

    public RenamedStreetNameRecord? GetRenamedStreetNameRecord(int streetNameLocalId)
    {
        return _dbContext.RenamedStreetNames.Find(streetNameLocalId);
    }
}
