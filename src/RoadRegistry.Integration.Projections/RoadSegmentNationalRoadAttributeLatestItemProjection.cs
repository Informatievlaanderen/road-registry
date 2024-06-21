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

public class RoadSegmentNationalRoadAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentNationalRoadAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfNationalRoads.Length == 0)
                return Task.CompletedTask;

            var nationalRoadAttributes = envelope.Message
                .PartOfNationalRoads
                .Select(nationalRoad => new RoadSegmentNationalRoadAttributeLatestItem
                {
                    Id = nationalRoad.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    Number = nationalRoad.Number,
                    BeginOrganizationId = nationalRoad.Origin.OrganizationId,
                    BeginOrganizationName = nationalRoad.Origin.Organization,
                    CreatedOnTimestamp = new DateTimeOffset(nationalRoad.Origin.Since),
                    VersionTimestamp = new DateTimeOffset(nationalRoad.Origin.Since)
                });

            return context.RoadSegmentNationalRoadAttributes.AddRangeAsync(nationalRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToNationalRoad nationalRoad:
                        await context.RoadSegmentNationalRoadAttributes.AddAsync(new RoadSegmentNationalRoadAttributeLatestItem
                        {
                            Id = nationalRoad.AttributeId,
                            RoadSegmentId = nationalRoad.SegmentId,
                            Number = nationalRoad.Number,
                            BeginOrganizationId = envelope.Message.OrganizationId,
                            BeginOrganizationName = envelope.Message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                        });
                        break;
                    case RoadSegmentRemovedFromNationalRoad nationalRoad:
                        var latestItem = await context.RoadSegmentNationalRoadAttributes
                            .FindAsync(nationalRoad.AttributeId, cancellationToken: token)
                            .ConfigureAwait(false);

                        if (latestItem is not null)
                        {
                            latestItem.IsRemoved = true;
                            latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
                            latestItem.BeginOrganizationName = envelope.Message.Organization;
                            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                        }
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        var latestItemsToRemove =
                            await context.RoadSegmentNationalRoadAttributes.IncludeLocalToListAsync(
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
