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

public class RoadSegmentSurfaceAttributeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentSurfaceAttributeLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.Surfaces.Length == 0)
                return Task.CompletedTask;

            var surfaceRecords = envelope.Message
                .Surfaces
                .Select(surfaceAttribute =>
                {
                    var surfaceTypeTranslation = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation;

                    return new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surfaceAttribute.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                        TypeId = surfaceTypeTranslation.Identifier,
                        TypeLabel = surfaceTypeTranslation.Name,
                        FromPosition = (double)surfaceAttribute.FromPosition,
                        ToPosition = (double)surfaceAttribute.ToPosition,
                        OrganizationId = surfaceAttribute.Origin.OrganizationId,
                        OrganizationName = surfaceAttribute.Origin.Organization,
                        CreatedOnTimestamp = new DateTimeOffset(surfaceAttribute.Origin.Since),
                        VersionTimestamp = new DateTimeOffset(surfaceAttribute.Origin.Since)
                    };
                });

            return context.RoadSegmentSurfaceAttributes.AddRangeAsync(surfaceRecords, token);
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
        if (segment.Surfaces.Length != 0)
        {
            var surfaces = segment
                .Surfaces
                .Select(surfaceAttribute =>
                {
                    var surfaceTypeTranslation = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation;

                    return new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surfaceAttribute.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                        TypeId = surfaceTypeTranslation.Identifier,
                        TypeLabel = surfaceTypeTranslation.Name,
                        FromPosition = (double)surfaceAttribute.FromPosition,
                        ToPosition = (double)surfaceAttribute.ToPosition,
                        OrganizationId = envelope.Message.OrganizationId,
                        OrganizationName = envelope.Message.Organization,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                    };
                });

            await context.RoadSegmentSurfaceAttributes.AddRangeAsync(surfaces, cancellationToken);
        }
    }

    private static async Task ModifyRoadSegment(
        IntegrationContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateSurfaces(context, envelope, segment.Id, segment.Surfaces, token);
    }

    private static async Task ModifyRoadSegmentAttributes(
        IntegrationContext context,
        RoadSegmentAttributesModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        if (segment.Surfaces is not null)
        {
            await UpdateSurfaces(context, envelope, segment.Id, segment.Surfaces, token);
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        IntegrationContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateSurfaces(context, envelope, segment.Id, segment.Surfaces, token);
    }

    private static async Task RemoveRoadSegment(
        IntegrationContext context,
        RoadSegmentRemoved segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItemsToRemove = await context.RoadSegmentSurfaceAttributes.IncludeLocalToListAsync(
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

    private static async Task UpdateSurfaces(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int roadSegmentId,
        RoadSegmentSurfaceAttributes[] surfaces,
        CancellationToken token)
    {
        var currentSet = (await context.RoadSegmentSurfaceAttributes.IncludeLocalToListAsync(
            q => q.Where(x => x.RoadSegmentId == roadSegmentId),
            token))
            .ToDictionary(a => a.Id);

        var nextSet = surfaces
            .Select(surfaceAttribute =>
            {
                var surfaceTypeTranslation = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation;

                return new RoadSegmentSurfaceAttributeLatestItem
                {
                    Id = surfaceAttribute.AttributeId,
                    RoadSegmentId = roadSegmentId,
                    AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                    TypeId = surfaceTypeTranslation.Identifier,
                    TypeLabel = surfaceTypeTranslation.Name,
                    FromPosition = (double)surfaceAttribute.FromPosition,
                    ToPosition = (double)surfaceAttribute.ToPosition,
                    OrganizationId = envelope.Message.OrganizationId,
                    OrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        context.RoadSegmentSurfaceAttributes.Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.Id = next.Id;
                current.RoadSegmentId = next.RoadSegmentId;
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.TypeId = next.TypeId;
                current.TypeLabel = next.TypeLabel;
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
