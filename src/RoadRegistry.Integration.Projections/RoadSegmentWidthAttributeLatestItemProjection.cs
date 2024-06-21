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

public class RoadSegmentWidthAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentWidthAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.Widths.Length == 0)
                return Task.CompletedTask;

            var widthRecords = envelope.Message
                .Widths
                .Select(widthAttribute =>
                {
                    var width = new RoadSegmentWidth(widthAttribute.Width);

                    return new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = widthAttribute.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                        Width = width,
                        WidthLabel = width.ToDutchString(),
                        FromPosition = (double)widthAttribute.FromPosition,
                        ToPosition = (double)widthAttribute.ToPosition,
                        BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                        BeginOrganizationName = envelope.Message.Origin.Organization,
                        CreatedOnTimestamp = new DateTimeOffset(envelope.Message.RecordingDate),
                        VersionTimestamp = new DateTimeOffset(envelope.Message.Origin.Since)
                    };
                });

            return context.RoadSegmentWidthAttributes.AddRangeAsync(widthRecords, token);
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
        if (segment.Widths.Length != 0)
        {
            var widths = segment
                .Widths
                .Select(widthAttribute =>
                {
                    var width = new RoadSegmentWidth(widthAttribute.Width);

                    return new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = widthAttribute.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                        Width = width,
                        WidthLabel = width.ToDutchString(),
                        FromPosition = (double)widthAttribute.FromPosition,
                        ToPosition = (double)widthAttribute.ToPosition,
                        BeginOrganizationId = envelope.Message.OrganizationId,
                        BeginOrganizationName = envelope.Message.Organization,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                    };
                });

            await context.RoadSegmentWidthAttributes.AddRangeAsync(widths, cancellationToken);
        }
    }

    private static async Task ModifyRoadSegment(
        IntegrationContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateWidths(context, envelope, segment.Id, segment.Widths, token);
    }

    private static async Task ModifyRoadSegmentAttributes(
        IntegrationContext context,
        RoadSegmentAttributesModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        if (segment.Widths is not null)
        {
            await UpdateWidths(context, envelope, segment.Id, segment.Widths, token);
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        IntegrationContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateWidths(context, envelope, segment.Id, segment.Widths, token);
    }

    private static async Task RemoveRoadSegment(
        IntegrationContext context,
        RoadSegmentRemoved segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItemsToRemove = await context.RoadSegmentWidthAttributes.IncludeLocalToListAsync(
            q => q.Where(x => x.RoadSegmentId == segment.Id),
            token);

        foreach (var latestItem in latestItemsToRemove)
        {
            latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
            latestItem.BeginOrganizationName = envelope.Message.Organization;
            latestItem.IsRemoved = true;
            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }

    private static async Task UpdateWidths(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int roadSegmentId,
        RoadSegmentWidthAttributes[] widths,
        CancellationToken token)
    {
        var currentSet = (await context.RoadSegmentWidthAttributes.IncludeLocalToListAsync(
            q => q.Where(x => x.RoadSegmentId == roadSegmentId),
            token))
            .ToDictionary(a => a.Id);

        var nextSet = widths
            .Select(widthAttribute =>
            {
                var width = new RoadSegmentWidth(widthAttribute.Width);

                return new RoadSegmentWidthAttributeLatestItem
                {
                    Id = widthAttribute.AttributeId,
                    RoadSegmentId = roadSegmentId,
                    AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                    Width = width,
                    WidthLabel = width.ToDutchString(),
                    FromPosition = (double)widthAttribute.FromPosition,
                    ToPosition = (double)widthAttribute.ToPosition,
                    BeginOrganizationId = envelope.Message.OrganizationId,
                    BeginOrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        context.RoadSegmentWidthAttributes.Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.Id = next.Id;
                current.RoadSegmentId = next.RoadSegmentId;
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.Width = next.Width;
                current.WidthLabel = next.WidthLabel;
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
