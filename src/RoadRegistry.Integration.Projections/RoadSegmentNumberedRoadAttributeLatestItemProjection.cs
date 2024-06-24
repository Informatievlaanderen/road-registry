namespace RoadRegistry.Integration.Projections;

using System;
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
                        CreatedOnTimestamp = new DateTimeOffset(numberedRoad.Origin.Since),
                        VersionTimestamp = new DateTimeOffset(numberedRoad.Origin.Since)
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
                        await RoadSegmentAdded(context, envelope, numberedRoad);
                        break;
                    case RoadSegmentOnNumberedRoadModified numberedRoad:
                        await RoadSegmentModified(context, envelope, numberedRoad, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad numberedRoad:
                        await RoadSegmentRemoved(context, envelope, numberedRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RoadSegmentRemoved(context, envelope, roadSegmentRemoved, token);
                        break;
                }
        });
    }

    private static async Task RoadSegmentAdded(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAddedToNumberedRoad numberedRoad)
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
        });
    }

    private static async Task RoadSegmentModified(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentOnNumberedRoadModified numberedRoad,
        CancellationToken token)
    {
        var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;

        var latestItem = await context.RoadSegmentNumberedRoadAttributes
            .FindAsync(numberedRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);

        latestItem!.Id = numberedRoad.AttributeId;
        latestItem.RoadSegmentId = numberedRoad.SegmentId;
        latestItem.Number = numberedRoad.Number;
        latestItem.DirectionId = directionTranslation.Identifier;
        latestItem.DirectionLabel = directionTranslation.Name;
        latestItem.SequenceNumber = numberedRoad.Ordinal;
        latestItem.OrganizationId = envelope.Message.OrganizationId;
        latestItem.OrganizationName = envelope.Message.Organization;
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RoadSegmentRemoved(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemovedFromNumberedRoad numberedRoad,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegmentNumberedRoadAttributes
            .FindAsync(numberedRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);

        if (latestItem is not null && !latestItem.IsRemoved)
        {
            latestItem.IsRemoved = true;
            latestItem.OrganizationId = envelope.Message.OrganizationId;
            latestItem.OrganizationName = envelope.Message.Organization;
            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }

    private static async Task RoadSegmentRemoved(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemoved roadSegmentRemoved,
        CancellationToken token)
    {

        var latestItemsToRemove =
            await context.RoadSegmentNumberedRoadAttributes.IncludeLocalToListAsync(
                q => q.Where(x =>
                    x.RoadSegmentId == roadSegmentRemoved.Id && !x.IsRemoved),
                token);

        foreach (var latestItemToRemove in latestItemsToRemove)
        {
            latestItemToRemove.IsRemoved = true;
            latestItemToRemove.OrganizationId = envelope.Message.OrganizationId;
            latestItemToRemove.OrganizationName = envelope.Message.Organization;
            latestItemToRemove.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
