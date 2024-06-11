namespace RoadRegistry.Integration.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Schema.RoadSegments;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadSegmentLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentLatestItemProjection()
    {
        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var geometry =
                GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var statusTranslation = RoadSegmentStatus.Parse(envelope.Message.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(envelope.Message.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(envelope.Message.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction).Translation;

            await context.RoadSegments.AddAsync(
                new RoadSegmentLatestItem
                {
                    Id = envelope.Message.Id,
                    StartNodeId = envelope.Message.StartNodeId,
                    EndNodeId = envelope.Message.EndNodeId,
                    Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),

                    Version = envelope.Message.Version,
                    GeometryVersion = envelope.Message.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    StatusLabel = statusTranslation.Name,
                    MorphologyId = morphologyTranslation.Identifier,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryId = categoryTranslation.Identifier,
                    CategoryLabel = categoryTranslation.Name,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    CreatedOnTimestamp = new DateTimeOffset(envelope.Message.RecordingDate),
                    VersionTimestamp = new DateTimeOffset(envelope.Message.Origin.Since),
                    BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                    BeginOrganizationName = envelope.Message.Origin.Organization
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape)),
                token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var message in envelope.Message.Changes.Flatten())
                switch (message)
                {
                    case RoadSegmentAdded roadSegmentAdded:
                        await AddRoadSegment(context, roadSegmentAdded, envelope, token);
                        break;

                    case RoadSegmentModified roadSegmentModified:
                        await ModifyRoadSegment(context, roadSegmentModified, envelope, token);
                        break;

                    case RoadSegmentAttributesModified roadSegmentAttributesModified:
                        await ModifyRoadSegmentAttributes(context, roadSegmentAttributesModified, envelope, token);
                        break;

                    case RoadSegmentGeometryModified roadSegmentGeometryModified:
                        await ModifyRoadSegmentGeometry(context, roadSegmentGeometryModified, envelope, token);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RemoveRoadSegment(context, roadSegmentRemoved, envelope, token);
                        break;
                }
        });
    }

    private static async Task AddRoadSegment(
        IntegrationContext context,
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id, token)
            .ConfigureAwait(false);
        if (latestItem is null)
        {
            latestItem = new RoadSegmentLatestItem
            {
                Id = roadSegmentAdded.Id
            };
            await context.RoadSegments.AddAsync(latestItem, token);
        }
        else
        {
            latestItem.IsRemoved = false;
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);

        latestItem.StartNodeId = roadSegmentAdded.StartNodeId;
        latestItem.EndNodeId = roadSegmentAdded.EndNodeId;
        latestItem.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        latestItem.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        latestItem.Version = roadSegmentAdded.Version;
        latestItem.GeometryVersion = roadSegmentAdded.GeometryVersion;
        latestItem.SetStatus(status);
        latestItem.SetMorphology(morphology);
        latestItem.SetCategory(category);
        latestItem.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        latestItem.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        latestItem.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        latestItem.SetMethod(geometryDrawMethod);
        latestItem.SetAccessRestriction(accessRestriction);

        latestItem.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
        latestItem.BeginOrganizationName = envelope.Message.Organization;
    }

    private static async Task ModifyRoadSegment(
        IntegrationContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id, token)
            .ConfigureAwait(false);
        if (latestItem is null)
        {
            latestItem = new RoadSegmentLatestItem
            {
                Id = roadSegmentModified.Id
            };
            await context.RoadSegments.AddAsync(latestItem, token);
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);

        latestItem.Id = roadSegmentModified.Id;
        latestItem.StartNodeId = roadSegmentModified.StartNodeId;
        latestItem.EndNodeId = roadSegmentModified.EndNodeId;
        latestItem.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry);
        latestItem.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        latestItem.Version = roadSegmentModified.Version;
        latestItem.GeometryVersion = roadSegmentModified.GeometryVersion;
        latestItem.SetStatus(status);
        latestItem.SetMorphology(morphology);
        latestItem.SetCategory(category);
        latestItem.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        latestItem.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        latestItem.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        latestItem.SetMethod(geometryDrawMethod);
        latestItem.SetAccessRestriction(accessRestriction);

        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task ModifyRoadSegmentAttributes(
        IntegrationContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAttributesModified.Id, token).ConfigureAwait(false);
        if (latestItem is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentLatestItem)} with id {roadSegmentAttributesModified.Id} is not found");
        }

        latestItem.Version = roadSegmentAttributesModified.Version;
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);
            latestItem.SetStatus(status);
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);
            latestItem.SetMorphology(morphology);
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);
            latestItem.SetCategory(category);
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);
            latestItem.SetAccessRestriction(accessRestriction);
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            latestItem.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        IntegrationContext context,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentGeometryModified.Id, token).ConfigureAwait(false);
        if (latestItem is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentLatestItem)} with id {roadSegmentGeometryModified.Id} is not found");
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);

        latestItem.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
        latestItem.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        latestItem.Version = roadSegmentGeometryModified.Version;
        latestItem.GeometryVersion = roadSegmentGeometryModified.GeometryVersion;
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RemoveRoadSegment(
        IntegrationContext context,
        RoadSegmentRemoved roadSegmentRemoved,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentRemoved.Id, token).ConfigureAwait(false);

        if (latestItem is not null && !latestItem.IsRemoved)
        {
            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            latestItem.IsRemoved = true;
        }
    }
}
