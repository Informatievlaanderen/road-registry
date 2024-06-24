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

public class RoadSegmentLaneAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentLaneAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.Lanes.Length == 0)
                return Task.CompletedTask;

            var laneRecords = envelope.Message
                .Lanes
                .Select(laneAttribute =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation;
                    return new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = laneAttribute.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                        Count = laneAttribute.Count,
                        DirectionId = laneDirectionTranslation.Identifier,
                        DirectionLabel = laneDirectionTranslation.Name,
                        FromPosition = (double)laneAttribute.FromPosition,
                        ToPosition = (double)laneAttribute.ToPosition,
                        OrganizationId = laneAttribute.Origin.OrganizationId,
                        OrganizationName = laneAttribute.Origin.Organization,
                        CreatedOnTimestamp = new DateTimeOffset(laneAttribute.Origin.Since),
                        VersionTimestamp = new DateTimeOffset(laneAttribute.Origin.Since)
                    };
                });

            return context.RoadSegmentLaneAttributes.AddRangeAsync(laneRecords, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAdded segment:
                        await AddRoadSegment(context, segment, envelope, token);
                        break;

                    case RoadSegmentModified segment:
                        await ModifyRoadSegment(context, segment, envelope, token);
                        break;

                    case RoadSegmentAttributesModified segment:
                        await ModifyRoadSegmentAttributes(context, segment, envelope, token);
                        break;

                    case RoadSegmentGeometryModified segment:
                        await ModifyRoadSegmentGeometry(context, segment, envelope, token);
                        break;

                    case RoadSegmentRemoved segment:
                        await RemoveRoadSegment(context, segment, envelope, token);
                        break;
                }
        });
    }

    private static async Task AddRoadSegment(
        IntegrationContext context,
        RoadSegmentAdded segment,
        Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken cancellationToken)
    {
        if (segment.Lanes.Length != 0)
        {
            var lanes = segment
                .Lanes
                .Select(laneAttribute =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation;
                    return new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = laneAttribute.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                        Count = laneAttribute.Count,
                        DirectionId = laneDirectionTranslation.Identifier,
                        DirectionLabel = laneDirectionTranslation.Name,
                        FromPosition = (double)laneAttribute.FromPosition,
                        ToPosition = (double)laneAttribute.ToPosition,
                        OrganizationId = envelope.Message.OrganizationId,
                        OrganizationName = envelope.Message.Organization,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                    };
                });

            await context.RoadSegmentLaneAttributes.AddRangeAsync(lanes, cancellationToken);
        }
    }

    private static async Task ModifyRoadSegment(
        IntegrationContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task ModifyRoadSegmentAttributes(
        IntegrationContext context,
        RoadSegmentAttributesModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        if (segment.Lanes is not null)
        {
            await UpdateLanes(context, envelope, segment.Id, segment.Lanes, token);
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        IntegrationContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task RemoveRoadSegment(
        IntegrationContext context,
        RoadSegmentRemoved segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItemsToRemove = await context.RoadSegmentLaneAttributes.IncludeLocalToListAsync(
            q => q.Where(x => x.RoadSegmentId == segment.Id),
            token);

        foreach (var latestItem in latestItemsToRemove)
        {
            latestItem.OrganizationId = envelope.Message.OrganizationId;
            latestItem.OrganizationName = envelope.Message.Organization;
            latestItem.IsRemoved = true;
            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }

    private static async Task UpdateLanes(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int roadSegmentId,
        RoadSegmentLaneAttributes[] lanes,
        CancellationToken token)
    {
        var currentSet = (await context.RoadSegmentLaneAttributes.IncludeLocalToListAsync(
            q => q.Where(x => x.RoadSegmentId == roadSegmentId),
            token))
            .ToDictionary(a => a.Id);

        var nextSet = lanes
            .Select(lane =>
            {
                var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                return new RoadSegmentLaneAttributeLatestItem
                {
                    Id = lane.AttributeId,
                    RoadSegmentId = roadSegmentId,
                    AsOfGeometryVersion = lane.AsOfGeometryVersion,
                    Count = lane.Count,
                    DirectionId = laneDirectionTranslation.Identifier,
                    DirectionLabel = laneDirectionTranslation.Name,
                    FromPosition = (double)lane.FromPosition,
                    ToPosition = (double)lane.ToPosition,
                    OrganizationId = envelope.Message.OrganizationId,
                    OrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        context.RoadSegmentLaneAttributes.Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.Id = next.Id;
                current.RoadSegmentId = next.RoadSegmentId;
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.Count = next.Count;
                current.DirectionId = next.DirectionId;
                current.DirectionLabel = next.DirectionLabel;
                current.FromPosition = next.FromPosition;
                current.ToPosition = next.ToPosition;
                current.OrganizationId = next.OrganizationId;
                current.OrganizationName = next.OrganizationName;
                current.VersionTimestamp = next.VersionTimestamp;
            },
            current =>
            {
                current.OrganizationId = envelope.Message.OrganizationId;
                current.OrganizationName = envelope.Message.Organization;
                current.IsRemoved = true;
                current.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            });
    }
}
