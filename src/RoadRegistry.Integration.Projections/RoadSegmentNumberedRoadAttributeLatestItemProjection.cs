namespace RoadRegistry.Integration.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.RoadSegments;

public class RoadSegmentNumberedRoadAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentNumberedRoadAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfNumberedRoads.Length == 0)
                return Task.CompletedTask;

            var numberedRoadAttributes = envelope.Message
                .PartOfNumberedRoads
                .Select(numberedRoad =>
                {
                    var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
                    return new RoadSegmentNumberedRoadAttributeLatestItem
                    {
                        Id = numberedRoad.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        Number = numberedRoad.Number,
                        DirectionId = directionTranslation.Identifier,
                        DirectionLabel = directionTranslation.Name,
                        SequenceNumber = numberedRoad.Ordinal,
                        OrganizationId = numberedRoad.Origin.OrganizationId,
                        OrganizationName = numberedRoad.Origin.Organization,
                        CreatedOnTimestamp = numberedRoad.Origin.Since.ToBelgianInstant(),
                        VersionTimestamp = numberedRoad.Origin.Since.ToBelgianInstant()
                    };
                });
            return context.RoadSegmentNumberedRoadAttributes.AddRangeAsync(numberedRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToNumberedRoad numberedRoad:
                        await RoadSegmentAddedToNumberedRoad(context, envelope, numberedRoad, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad numberedRoad:
                        await RoadSegmentRemovedFromNumberedRoad(context, envelope, numberedRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RoadSegmentRemoved(context, envelope, roadSegmentRemoved, token);
                        break;
                }
        });
    }

    private static async Task RoadSegmentAddedToNumberedRoad(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAddedToNumberedRoad numberedRoad,
        CancellationToken token)
    {
        var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;

        await context.RoadSegmentNumberedRoadAttributes.AddAsync(new RoadSegmentNumberedRoadAttributeLatestItem
        {
            Id = numberedRoad.AttributeId,
            RoadSegmentId = numberedRoad.SegmentId,
            Number = numberedRoad.Number,
            DirectionId = directionTranslation.Identifier,
            DirectionLabel = directionTranslation.Name,
            SequenceNumber = numberedRoad.Ordinal,
            OrganizationId = envelope.Message.OrganizationId,
            OrganizationName = envelope.Message.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        }, token);
    }

    private static async Task RoadSegmentRemovedFromNumberedRoad(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemovedFromNumberedRoad numberedRoad,
        CancellationToken token)
    {
        var item = await context.RoadSegmentNumberedRoadAttributes
            .FindAsync(numberedRoad.AttributeId, cancellationToken: token)
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
            await context.RoadSegmentNumberedRoadAttributes.IncludeLocalToListAsync(
                q => q.Where(x =>
                    x.RoadSegmentId == roadSegmentRemoved.Id && !x.IsRemoved),
                token);

        foreach (var item in itemsToRemove)
        {
            item.IsRemoved = true;
            item.OrganizationId = envelope.Message.OrganizationId;
            item.OrganizationName = envelope.Message.Organization;
            item.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
