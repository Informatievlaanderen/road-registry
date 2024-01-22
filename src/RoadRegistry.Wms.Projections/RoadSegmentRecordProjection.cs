namespace RoadRegistry.Wms.Projections;

using BackOffice;
using BackOffice.Abstractions;
using BackOffice.Extensions;
using BackOffice.FeatureToggles;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
{
    public RoadSegmentRecordProjection(IStreetNameCache streetNameCache, UseRoadSegmentSoftDeleteFeatureToggle useRoadSegmentSoftDeleteFeatureToggle)
    {
        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
            var status = RoadSegmentStatus.Parse(envelope.Message.Status);
            var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
            var category = RoadSegmentCategory.Parse(envelope.Message.Category);
            var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.RightSide.StreetNameId, token);

            await context.RoadSegments.AddAsync(new RoadSegmentRecord
            {
                Id = envelope.Message.Id,
                BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                BeginOrganizationName = envelope.Message.Origin.Organization,
                BeginTime = envelope.Message.Origin.Since,
                BeginApplication = envelope.Message.Origin.Application,

                MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                MaintainerName = envelope.Message.MaintenanceAuthority.Name,

                MethodId = method.Translation.Identifier,
                MethodDutchName = method.Translation.Name,

                CategoryId = category.Translation.Identifier,
                CategoryDutchName = category.Translation.Name,

                Geometry2D = WmsGeometryTranslator.Translate2D(envelope.Message.Geometry),
                GeometryVersion = envelope.Message.GeometryVersion,

                MorphologyId = morphology.Translation.Identifier,
                MorphologyDutchName = morphology.Translation.Name,

                StatusId = status.Translation.Identifier,
                StatusDutchName = status.Translation.Name,

                AccessRestrictionId = accessRestriction.Translation.Identifier,
                AccessRestrictionDutchName = accessRestriction.Translation.Name,

                RecordingDate = envelope.Message.RecordingDate,
                TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32(),

                LeftSideMunicipalityId = null,
                LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode,
                LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                LeftSideStreetName = leftSideStreetNameRecord?.Name ??
                                     envelope.Message.LeftSide.StreetName,
                RightSideMunicipalityId = null,
                RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
                RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                RightSideStreetName = rightSideStreetNameRecord?.Name ??
                                      envelope.Message.RightSide.StreetName,

                RoadSegmentVersion = envelope.Message.Version,

                BeginRoadNodeId = envelope.Message.StartNodeId,
                EndRoadNodeId = envelope.Message.EndNodeId
            }, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAdded roadSegmentAdded:
                        await AddRoadSegment(streetNameCache, context, envelope, roadSegmentAdded, token);
                        break;

                    case RoadSegmentModified roadSegmentModified:
                        await ModifyRoadSegment(streetNameCache, context, envelope, roadSegmentModified, token);
                        break;

                    case RoadSegmentAttributesModified roadSegmentAttributesModified:
                        await ModifyRoadSegmentAttributes(context, envelope, roadSegmentAttributesModified, token);
                        break;

                    case RoadSegmentGeometryModified roadSegmentGeometryModified:
                        await ModifyRoadSegmentGeometry(context, envelope, roadSegmentGeometryModified, token);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RemoveRoadSegment(context, envelope, roadSegmentRemoved, useRoadSegmentSoftDeleteFeatureToggle.FeatureEnabled, token);
                        break;
                }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            if (envelope.Message.NameModified)
            {
                await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
            }
        });

        When<Envelope<StreetNameModified>>(async (context, envelope, token) =>
        {
            if (envelope.Message.NameModified)
            {
                await UpdateStreetNameLabels(context, new StreetNameLocalId(envelope.Message.Record.PersistentLocalId), envelope.Message.Record.DutchName, token);
            }
        });
    }

    private static async Task AddRoadSegment(IStreetNameCache streetNameCache,
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAdded roadSegmentAdded,
        CancellationToken token)
    {
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);

        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);

        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);

        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);

        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

        var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.RightSide.StreetNameId, token);

        await context.RoadSegments.AddAsync(new RoadSegmentRecord
        {
            BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            BeginOrganizationId = envelope.Message.OrganizationId,
            BeginOrganizationName = envelope.Message.Organization,

            Id = roadSegmentAdded.Id,
            BeginApplication = null,

            MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
            MaintainerName = roadSegmentAdded.MaintenanceAuthority.Name,

            MethodId = method.Translation.Identifier,
            MethodDutchName = method.Translation.Name,

            CategoryId = category.Translation.Identifier,
            CategoryDutchName = category.Translation.Name,

            Geometry2D = WmsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry),
            GeometryVersion = roadSegmentAdded.GeometryVersion,

            MorphologyId = morphology.Translation.Identifier,
            MorphologyDutchName = morphology.Translation.Name,

            StatusId = status.Translation.Identifier,
            StatusDutchName = status.Translation.Name,

            AccessRestrictionId = accessRestriction.Translation.Identifier,
            AccessRestrictionDutchName = accessRestriction.Translation.Name,

            RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32(),

            LeftSideMunicipalityId = null,
            LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode,
            LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
            LeftSideStreetName = leftSideStreetNameRecord?.Name,

            RightSideMunicipalityId = null,
            RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
            RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
            RightSideStreetName = rightSideStreetNameRecord?.Name,

            RoadSegmentVersion = roadSegmentAdded.Version,

            BeginRoadNodeId = roadSegmentAdded.StartNodeId,
            EndRoadNodeId = roadSegmentAdded.EndNodeId
        }, token);
    }

    private static async Task ModifyRoadSegment(IStreetNameCache streetNameCache,
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentModified roadSegmentModified,
        CancellationToken token)
    {
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        var method =
            RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);

        var accessRestriction =
            RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);

        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);

        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);

        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

        var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.RightSide.StreetNameId, token);

        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found");
        }

        roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        roadSegmentRecord.Id = roadSegmentModified.Id;
        roadSegmentRecord.BeginApplication = null;

        roadSegmentRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        roadSegmentRecord.MaintainerName = roadSegmentModified.MaintenanceAuthority.Name;

        roadSegmentRecord.MethodId = method.Translation.Identifier;
        roadSegmentRecord.MethodDutchName = method.Translation.Name;

        roadSegmentRecord.CategoryId = category.Translation.Identifier;
        roadSegmentRecord.CategoryDutchName = category.Translation.Name;

        roadSegmentRecord.Geometry2D = WmsGeometryTranslator.Translate2D(roadSegmentModified.Geometry);
        roadSegmentRecord.GeometryVersion = roadSegmentModified.GeometryVersion;

        roadSegmentRecord.MorphologyId = morphology.Translation.Identifier;
        roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;

        roadSegmentRecord.StatusId = status.Translation.Identifier;
        roadSegmentRecord.StatusDutchName = status.Translation.Name;

        roadSegmentRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
        roadSegmentRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

        roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

        roadSegmentRecord.LeftSideMunicipalityId = null;
        roadSegmentRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
        roadSegmentRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        roadSegmentRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

        roadSegmentRecord.RightSideMunicipalityId = null;
        roadSegmentRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
        roadSegmentRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        roadSegmentRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

        roadSegmentRecord.RoadSegmentVersion = roadSegmentModified.Version;

        roadSegmentRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
        roadSegmentRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
    }

    private static async Task ModifyRoadSegmentAttributes(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentAttributesModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
        }

        roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

            roadSegmentRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            roadSegmentRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;
        }

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

            roadSegmentRecord.StatusId = status.Translation.Identifier;
            roadSegmentRecord.StatusDutchName = status.Translation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

            roadSegmentRecord.MorphologyId = morphology.Translation.Identifier;
            roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

            roadSegmentRecord.CategoryId = category.Translation.Identifier;
            roadSegmentRecord.CategoryDutchName = category.Translation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            roadSegmentRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            roadSegmentRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
        }

        roadSegmentRecord.RoadSegmentVersion = roadSegmentAttributesModified.Version;

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();
    }

    private static async Task ModifyRoadSegmentGeometry(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentGeometryModified segment,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(segment.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
        }

        roadSegmentRecord.Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry);
        roadSegmentRecord.GeometryVersion = segment.GeometryVersion;

        roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        roadSegmentRecord.RoadSegmentVersion = segment.Version;

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();
    }

    private static async Task RemoveRoadSegment(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemoved roadSegmentRemoved,
        bool softDelete,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord is not null)
        {
            if (softDelete)
            {
                if (!roadSegmentRecord.IsRemoved)
                {
                    roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                    roadSegmentRecord.IsRemoved = true;
                }
            }
            else
            {
                context.RoadSegments.Remove(roadSegmentRecord);
            }
        }
    }

    private async Task RenameOrganization(
        WmsContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        await context.RoadSegments
            .ForEachBatchAsync(q => q
                .Where(x => x.MaintainerId == organizationId), 5000, dbRecords =>
            {
                foreach (var dbRecord in dbRecords)
                {
                    dbRecord.MaintainerName = organizationName;
                }

                return Task.CompletedTask;
            }, cancellationToken);
    }

    private async Task UpdateStreetNameLabels(
        WmsContext context,
        StreetNameLocalId streetNameLocalId,
        string dutchName,
        CancellationToken cancellationToken)
    {
        await context.RoadSegments.ForEachBatchAsync(q =>
                q.Where(x => x.LeftSideStreetNameId == streetNameLocalId || x.RightSideStreetNameId == streetNameLocalId),
            5000,
            dbRecords =>
            {
                foreach (var dbRecord in dbRecords)
                {
                    if (dbRecord.LeftSideStreetNameId == streetNameLocalId)
                    {
                        dbRecord.LeftSideStreetName = dutchName;
                    }

                    if (dbRecord.RightSideStreetNameId == streetNameLocalId)
                    {
                        dbRecord.RightSideStreetName = dutchName;
                    }
                }

                return Task.CompletedTask;
            }, cancellationToken);
    }

    private static async Task<StreetNameCacheItem> TryGetFromCache(
        IStreetNameCache streetNameCache,
        int? streetNameId,
        CancellationToken token)
    {
        return streetNameId.HasValue ? await streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) : null;
    }
}
