namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Models;

public class StreetNameEventProjection : ConnectedProjection<StreetNameEventProjectionContext>
{
    public StreetNameEventProjection()
    {
        When<Envelope<StreetNameRenamed>>(StreetNameRenamed);
    }

    private async Task StreetNameRenamed(StreetNameEventProjectionContext context, Envelope<StreetNameRenamed> envelope, CancellationToken token)
    {
        var dbRecord = await context.RenamedStreetNames.FindAsync(new object[] { envelope.Message.StreetNameLocalId }, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            await context.RenamedStreetNames.AddAsync(new RenamedStreetNameRecord
            {
                StreetNameLocalId = envelope.Message.StreetNameLocalId,
                DestinationStreetNameLocalId = envelope.Message.DestinationStreetNameLocalId
            }, token);
        }
    }
}
