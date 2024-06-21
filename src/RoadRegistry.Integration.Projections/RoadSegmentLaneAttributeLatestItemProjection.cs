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
using Microsoft.EntityFrameworkCore;
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
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        Version = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = laneDirectionTranslation.Identifier,
                        DirectionLabel = laneDirectionTranslation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                        BeginOrganizationName = envelope.Message.Origin.Organization,
                        CreatedOnTimestamp = new DateTimeOffset(envelope.Message.RecordingDate),
                        VersionTimestamp = new DateTimeOffset(envelope.Message.Origin.Since)
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
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = segment.Id,
                        Version = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = laneDirectionTranslation.Identifier,
                        DirectionLabel = laneDirectionTranslation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        BeginOrganizationId = envelope.Message.OrganizationId,
                        BeginOrganizationName = envelope.Message.Organization,
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
        var latestItemsToRemove = context
            .RoadSegmentLaneAttributes.Local
            .Where(a => a.RoadSegmentId == segment.Id)
            .Concat(await context
                .RoadSegmentLaneAttributes
                .Where(a => a.RoadSegmentId == segment.Id)
                .ToArrayAsync(token)
            );

        foreach (var latestItem in latestItemsToRemove)
        {
            latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
            latestItem.BeginOrganizationName = envelope.Message.Organization;
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
        //Causes all attributes to be loaded into Local
        await context
            .RoadSegmentLaneAttributes
            .Where(a => a.RoadSegmentId == roadSegmentId)
            .ToArrayAsync(token);
        var currentSet = context
            .RoadSegmentLaneAttributes
            .Local.Where(a => a.RoadSegmentId == roadSegmentId)
            .ToDictionary(a => a.Id);

        var nextSet = lanes
            .Select(lane =>
            {
                var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                return new RoadSegmentLaneAttributeLatestItem
                {
                    Id = lane.AttributeId,
                    RoadSegmentId = roadSegmentId,
                    Version = lane.AsOfGeometryVersion,
                    Count = lane.Count,
                    DirectionId = laneDirectionTranslation.Identifier,
                    DirectionLabel = laneDirectionTranslation.Name,
                    FromPosition = (double)lane.FromPosition,
                    ToPosition = (double)lane.ToPosition,
                    BeginOrganizationId = envelope.Message.OrganizationId,
                    BeginOrganizationName = envelope.Message.Organization,
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
                current.Version = next.Version;
                current.Count = next.Count;
                current.DirectionId = next.DirectionId;
                current.DirectionLabel = next.DirectionLabel;
                current.FromPosition = next.FromPosition;
                current.ToPosition = next.ToPosition;
                current.BeginOrganizationId = next.BeginOrganizationId;
                current.BeginOrganizationName = next.BeginOrganizationName;
                current.VersionTimestamp = next.VersionTimestamp;
            },
            current =>
            {
                current.BeginOrganizationId = envelope.Message.OrganizationId;
                current.BeginOrganizationName = envelope.Message.Organization;
                current.IsRemoved = true;
                current.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            });
    }
}
