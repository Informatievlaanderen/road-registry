namespace RoadRegistry.SyncHost.Tests.StreetName.Projections.Fixtures;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Sync.StreetNameRegistry;

public class StreetNameEventConsumerProjectionFixture : ConnectedProjectionFixture<StreetNameEventConsumerProjection, StreetNameEventConsumerContext>
{
    private readonly StreetNameEventConsumerContext _dbContext;

    public StreetNameEventConsumerProjectionFixture(StreetNameEventConsumerContext context)
        : base(context, Resolve.WhenEqualToHandlerMessageType(new StreetNameEventConsumerProjection().Handlers))
    {
        _dbContext = context;
    }

    public RenamedStreetNameRecord? GetRenamedStreetNameRecord(int streetNameLocalId)
    {
        return _dbContext.RenamedStreetNames.Find(streetNameLocalId);
    }
}
