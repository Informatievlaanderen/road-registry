namespace RoadRegistry.Syndication.Projections
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;
    using StreetNameEvents;

    public class StreetNameCacheProjection : ConnectedProjection<SyndicationContext>
    {
        public StreetNameCacheProjection()
        {
            When<Envelope<StreetNameWasRegistered>>(async (context, envelope, token) =>
            {
                // await context.Municipalities.AddAsync(
                //     new StreetNameRecord
                //     {
                //         StreetNameId = envelope.Message.StreetNameId,
                //         MunicipalityId = envelope.Message.MunicipalityId,
                //         NisCode = envelope.Message.NisCode,
                //     }, token);
            });

        }
    }
}
