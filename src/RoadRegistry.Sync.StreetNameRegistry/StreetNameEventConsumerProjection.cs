namespace RoadRegistry.Sync.StreetNameRegistry;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using System.Threading;
using System.Threading.Tasks;

public class StreetNameEventConsumerProjection : ConnectedProjection<StreetNameEventConsumerContext>
{
    public StreetNameEventConsumerProjection()
    {
        When<StreetNameWasRenamed>(StreetNameWasRenamed);
    }

    private async Task StreetNameWasRenamed(StreetNameEventConsumerContext context, StreetNameWasRenamed message, CancellationToken token)
    {
        var dbRecord = await context.RenamedStreetNames.FindAsync(new object[] { message.PersistentLocalId }, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            await context.RenamedStreetNames.AddAsync(new RenamedStreetNameRecord
            {
                StreetNameLocalId = message.PersistentLocalId,
                DestinationStreetNameLocalId = message.DestinationPersistentLocalId
            }, token);
        }
    }
}
