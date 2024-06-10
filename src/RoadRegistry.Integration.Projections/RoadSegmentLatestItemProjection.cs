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
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging;
using Schema;
using Schema.RoadSegments;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadSegmentLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    private readonly ILogger<RoadSegmentLatestItemProjection> _logger;

    public RoadSegmentLatestItemProjection(
        ILogger<RoadSegmentLatestItemProjection> logger)
    {
        _logger = logger.ThrowIfNull();

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
            var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

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
                    MaintainerName = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name),
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    TransactionId = transactionId,
                    RecordingDate = envelope.Message.RecordingDate,
                    BeginTime = envelope.Message.Origin.Since,
                    BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                    BeginOrganizationName = envelope.Message.Origin.Organization,

                    // Puri = TODO
                    // Namespace =
                    // VersionTimestamp =

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

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            _logger.LogInformation("{Message} started", envelope.Message.GetType().Name);
            if (envelope.Message.NameModified)
            {
                await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
            }
            _logger.LogInformation("{Message} finished", envelope.Message.GetType().Name);
        });
    }

    private static async Task AddRoadSegment(
        IntegrationContext context,
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalWithoutQueryFiltersSingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentLatestItem
            {
                Id = roadSegmentAdded.Id
            };
            await context.RoadSegments.AddAsync(dbRecord, token);
        }
        else
        {
            dbRecord.IsRemoved = false;
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        dbRecord.StartNodeId = roadSegmentAdded.StartNodeId;
        dbRecord.EndNodeId = roadSegmentAdded.EndNodeId;
        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        dbRecord.Version = roadSegmentAdded.Version;
        dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;
        dbRecord.SetStatus(status);
        dbRecord.SetMorphology(morphology);
        dbRecord.SetCategory(category);
        dbRecord.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        dbRecord.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        dbRecord.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);
        dbRecord.SetMethod(geometryDrawMethod);
        dbRecord.SetAccessRestriction(accessRestriction);

        dbRecord.TransactionId = transactionId;
        dbRecord.RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.BeginOrganizationId = envelope.Message.OrganizationId;
        dbRecord.BeginOrganizationName = envelope.Message.Organization;
    }

    private static async Task ModifyRoadSegment(
        IntegrationContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalWithoutQueryFiltersSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentLatestItem
            {
                Id = roadSegmentModified.Id
            };
            await context.RoadSegments.AddAsync(dbRecord, token);
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        dbRecord.Id = roadSegmentModified.Id;
        dbRecord.StartNodeId = roadSegmentModified.StartNodeId;
        dbRecord.EndNodeId = roadSegmentModified.EndNodeId;
        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        dbRecord.Version = roadSegmentModified.Version;
        dbRecord.GeometryVersion = roadSegmentModified.GeometryVersion;
        dbRecord.SetStatus(status);
        dbRecord.SetMorphology(morphology);
        dbRecord.SetCategory(category);
        dbRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        dbRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        dbRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);
        dbRecord.SetMethod(geometryDrawMethod);
        dbRecord.SetAccessRestriction(accessRestriction);

        dbRecord.TransactionId = transactionId;
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task ModifyRoadSegmentAttributes(
        IntegrationContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAttributesModified.Id, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentLatestItem)} with id {roadSegmentAttributesModified.Id} is not found");
        }

        dbRecord.Version = roadSegmentAttributesModified.Version;
        dbRecord.TransactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);
            dbRecord.SetStatus(status);
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);
            dbRecord.SetMorphology(morphology);
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);
            dbRecord.SetCategory(category);
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);
            dbRecord.SetAccessRestriction(accessRestriction);
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            dbRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAttributesModified.MaintenanceAuthority.Name);
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        IntegrationContext context,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentGeometryModified.Id, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentLatestItem)} with id {roadSegmentGeometryModified.Id} is not found");
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);

        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        dbRecord.Version = roadSegmentGeometryModified.Version;
        dbRecord.GeometryVersion = roadSegmentGeometryModified.GeometryVersion;
        dbRecord.TransactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RemoveRoadSegment(
        IntegrationContext context,
        RoadSegmentRemoved roadSegmentRemoved,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentRemoved.Id, token).ConfigureAwait(false);

        if (dbRecord is not null && !dbRecord.IsRemoved)
        {
            dbRecord.TransactionId = new TransactionId(envelope.Message.TransactionId);
            dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            dbRecord.IsRemoved = true;
        }
    }

    private async Task RenameOrganization(
        IntegrationContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Renaming organizations started");
        var batchIndex = 0;

        var organizationIdValue = organizationId.ToString();

        await context.RoadSegments
            .ForEachBatchAsync(q => q.Where(x => x.MaintainerId == organizationIdValue), 5000, dbRecords =>
            {
                _logger.LogInformation("Processing batch {Batch}", batchIndex);

                foreach (var dbRecord in dbRecords)
                {
                    dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(organizationName);
                }

                batchIndex++;
                return Task.CompletedTask;
            }, cancellationToken);

        _logger.LogInformation("Renaming organizations finished");
    }
}
