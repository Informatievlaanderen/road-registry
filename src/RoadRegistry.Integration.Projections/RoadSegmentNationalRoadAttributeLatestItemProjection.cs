namespace RoadRegistry.Integration.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.RoadSegments;

public class RoadSegmentNationalRoadAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentNationalRoadAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfNationalRoads.Length == 0)
            {
                return Task.CompletedTask;
            }

            var nationalRoadAttributes = envelope.Message
                .PartOfNationalRoads
                .Select(nationalRoad => new RoadSegmentNationalRoadAttributeLatestItem
                {
                    Id = nationalRoad.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    Number = nationalRoad.Number,
                    OrganizationId = nationalRoad.Origin.OrganizationId,
                    OrganizationName = nationalRoad.Origin.Organization,
                    CreatedOnTimestamp = nationalRoad.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = nationalRoad.Origin.Since.ToBelgianInstant()
                });

            return context.RoadSegmentNationalRoadAttributes.AddRangeAsync(nationalRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToNationalRoad nationalRoad:
                        await RoadSegmentAddedToNationalRoad(context, envelope, nationalRoad, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad nationalRoad:
                        await RoadSegmentRemovedFromNationalRoad(context, envelope, nationalRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RoadSegmentRemoved(context, envelope, roadSegmentRemoved, token);
                        break;
                }
        });
    }

    private static async Task RoadSegmentAddedToNationalRoad(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAddedToNationalRoad nationalRoad,
        CancellationToken token)
    {
        await context.RoadSegmentNationalRoadAttributes.AddAsync(new RoadSegmentNationalRoadAttributeLatestItem
        {
            Id = nationalRoad.AttributeId,
            RoadSegmentId = nationalRoad.SegmentId,
            Number = nationalRoad.Number,
            OrganizationId = envelope.Message.OrganizationId,
            OrganizationName = envelope.Message.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        }, token);
    }

    private static async Task RoadSegmentRemovedFromNationalRoad(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemovedFromNationalRoad nationalRoad,
        CancellationToken token)
    {
        var item = await context.RoadSegmentNationalRoadAttributes
            .FindAsync(nationalRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);

        if (item is not null && !item.IsRemoved)
        {
            item.IsRemoved = true;
            item.OrganizationId = envelope.Message.OrganizationId;
            item.OrganizationName = envelope.Message.Organization;
            item.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }

    private static async Task RoadSegmentRemoved(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemoved roadSegmentRemoved,
        CancellationToken token)
    {
        var itemsToRemove =
            await context.RoadSegmentNationalRoadAttributes.IncludeLocalToListAsync(
                q => q.Where(x =>
                    x.RoadSegmentId == roadSegmentRemoved.Id && !x.IsRemoved),
                token);

        foreach (var itemToRemove in itemsToRemove)
        {
            itemToRemove.IsRemoved = true;
            itemToRemove.OrganizationId = envelope.Message.OrganizationId;
            itemToRemove.OrganizationName = envelope.Message.Organization;
            itemToRemove.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
