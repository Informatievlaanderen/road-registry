namespace RoadRegistry.Integration.Projections;

using System;
using System.Linq;
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
                    BeginOrganizationId = europeanRoad.Origin.OrganizationId,
                    BeginOrganizationName = europeanRoad.Origin.Organization,
                    CreatedOnTimestamp = new DateTimeOffset(europeanRoad.Origin.Since),
                    VersionTimestamp = new DateTimeOffset(europeanRoad.Origin.Since)
                });

            return context.AddRangeAsync(europeanRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToEuropeanRoad europeanRoad:
                        await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeLatestItem
                        {
                            Id = europeanRoad.AttributeId,
                            RoadSegmentId = europeanRoad.SegmentId,
                            Number = europeanRoad.Number,
                            BeginOrganizationId = envelope.Message.OrganizationId,
                            BeginOrganizationName = envelope.Message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                        });
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
                        var latestItem =
                            await context.RoadSegmentEuropeanRoadAttributes
                                .FindAsync(europeanRoad.AttributeId, cancellationToken: token)
                                .ConfigureAwait(false);

                        if (latestItem is not null && !latestItem.IsRemoved)
                        {
                            latestItem.IsRemoved = true;
                            latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
                            latestItem.BeginOrganizationName = envelope.Message.Organization;
                            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                        }
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        var latestItemsToRemove =
                            await context.RoadSegmentEuropeanRoadAttributes.IncludeLocalToListAsync(
                                q => q.Where(x => x.RoadSegmentId == roadSegmentRemoved.Id),
                                token);

                        foreach (var latestItemToRemove in latestItemsToRemove)
                        {
                            latestItemToRemove.IsRemoved = true;
                            latestItemToRemove.BeginOrganizationId = envelope.Message.OrganizationId;
                            latestItemToRemove.BeginOrganizationName = envelope.Message.Organization;
                            latestItemToRemove.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                        }
                        break;
                }
        });
    }
}
