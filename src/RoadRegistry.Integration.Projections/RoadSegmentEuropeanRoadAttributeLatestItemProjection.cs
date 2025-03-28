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

public class RoadSegmentEuropeanRoadAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentEuropeanRoadAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfEuropeanRoads.Length == 0)
                return Task.CompletedTask;

            var europeanRoadAttributes = envelope.Message
                .PartOfEuropeanRoads
                .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeLatestItem
                {
                    Id = europeanRoad.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    Number = europeanRoad.Number,
                    OrganizationId = europeanRoad.Origin.OrganizationId,
                    OrganizationName = europeanRoad.Origin.Organization,
                    CreatedOnTimestamp = europeanRoad.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = europeanRoad.Origin.Since.ToBelgianInstant()
                });

            return context.AddRangeAsync(europeanRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToEuropeanRoad europeanRoad:
                        await RoadSegmentAddedToEuropeanRoad(context, envelope, europeanRoad, token);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
                        await RoadSegmentRemovedFromEuropeanRoad(context, envelope, europeanRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RoadSegmentRemoved(context, envelope, roadSegmentRemoved, token);
                        break;
                }
        });
    }

    private static async Task RoadSegmentAddedToEuropeanRoad(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAddedToEuropeanRoad europeanRoad,
        CancellationToken token)
    {
        await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeLatestItem
        {
            Id = europeanRoad.AttributeId,
            RoadSegmentId = europeanRoad.SegmentId,
            Number = europeanRoad.Number,
            OrganizationId = envelope.Message.OrganizationId,
            OrganizationName = envelope.Message.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        }, token);
    }

    private static async Task RoadSegmentRemovedFromEuropeanRoad(IntegrationContext context, Envelope<RoadNetworkChangesAccepted> envelope, RoadSegmentRemovedFromEuropeanRoad europeanRoad, CancellationToken token)
    {
        var item = await context.RoadSegmentEuropeanRoadAttributes
            .FindAsync(europeanRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);

        if (item is not null && !item.IsRemoved)
        {
            item.IsRemoved = true;
            item.OrganizationId = envelope.Message.OrganizationId;
            item.OrganizationName = envelope.Message.Organization;
            item.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }

    private static async Task RoadSegmentRemoved(IntegrationContext context, Envelope<RoadNetworkChangesAccepted> envelope, RoadSegmentRemoved roadSegmentRemoved, CancellationToken token)
    {
        var itemsToRemove =
            await context.RoadSegmentEuropeanRoadAttributes.IncludeLocalToListAsync(
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
