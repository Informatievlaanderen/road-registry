namespace RoadRegistry.Wfs.Projections;

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

public class RoadSegmentRecordProjection : ConnectedProjection<WfsContext>
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
            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.RightSide.StreetNameId, token);

            await context.RoadSegments.AddAsync(new RoadSegmentRecord
            {
                Id = envelope.Message.Id,
                BeginTime = envelope.Message.Origin.Since,
                MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                MaintainerName = envelope.Message.MaintenanceAuthority.Name,
                MethodDutchName = method.Translation.Name,
                CategoryDutchName = category.Translation.Name,
                Geometry2D = WfsGeometryTranslator.Translate2D(envelope.Message.Geometry),
                MorphologyDutchName = morphology.Translation.Name,
                StatusDutchName = status.Translation.Name,
                AccessRestriction = accessRestriction.Translation.Name,
                LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                LeftSideStreetName = leftSideStreetNameRecord?.Name ??
                                     envelope.Message.LeftSide.StreetName,
                RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                RightSideStreetName = rightSideStreetNameRecord?.Name ??
                                      envelope.Message.RightSide.StreetName,
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
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAdded roadSegmentAdded,
        CancellationToken token)
    {
        var removedRecord = await context.RoadSegments.SingleOrDefaultIncludingLocalAsync(x => x.Id == roadSegmentAdded.Id && x.IsRemoved, token).ConfigureAwait(false);
        if (removedRecord is not null)
        {
            context.RoadSegments.Remove(removedRecord);
        }

        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

        var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.RightSide.StreetNameId, token);

        await context.RoadSegments.AddAsync(UpdateBeginTime(new RoadSegmentRecord
        {
            Id = roadSegmentAdded.Id,
            MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
            MaintainerName = roadSegmentAdded.MaintenanceAuthority.Name,
            MethodDutchName = method.Translation.Name,
            CategoryDutchName = category.Translation.Name,
            Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry),
            MorphologyDutchName = morphology.Translation.Name,
            StatusDutchName = status.Translation.Name,
            AccessRestriction = accessRestriction.Translation.Name,
            LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
            LeftSideStreetName = leftSideStreetNameRecord?.Name,
            RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
            RightSideStreetName = rightSideStreetNameRecord?.Name,
            BeginRoadNodeId = roadSegmentAdded.StartNodeId,
            EndRoadNodeId = roadSegmentAdded.EndNodeId
        }, envelope), token);
    }

    private static async Task ModifyRoadSegment(IStreetNameCache streetNameCache,
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentModified roadSegmentModified,
        CancellationToken token)
    {
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);
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

        roadSegmentRecord.Id = roadSegmentModified.Id;
        roadSegmentRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        roadSegmentRecord.MaintainerName = roadSegmentModified.MaintenanceAuthority.Name;
        roadSegmentRecord.MethodDutchName = method.Translation.Name;
        roadSegmentRecord.CategoryDutchName = category.Translation.Name;
        roadSegmentRecord.Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentModified.Geometry);
        roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;
        roadSegmentRecord.StatusDutchName = status.Translation.Name;
        roadSegmentRecord.AccessRestriction = accessRestriction.Translation.Name;
        roadSegmentRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        roadSegmentRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;
        roadSegmentRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        roadSegmentRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;
        roadSegmentRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
        roadSegmentRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
        UpdateBeginTime(roadSegmentRecord, envelope);
    }

    private static async Task ModifyRoadSegmentAttributes(
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentAttributesModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

            roadSegmentRecord.AccessRestriction = accessRestriction.Translation.Name;
        }

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

            roadSegmentRecord.StatusDutchName = status.Translation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

            roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

            roadSegmentRecord.CategoryDutchName = category.Translation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            roadSegmentRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            roadSegmentRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
        }

        UpdateBeginTime(roadSegmentRecord, envelope);
    }

    private static async Task ModifyRoadSegmentGeometry(
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentGeometryModified segment,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(segment.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
        }

        roadSegmentRecord.Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry);

        UpdateBeginTime(roadSegmentRecord, envelope);
    }

    private static async Task RemoveRoadSegment(
        WfsContext context,
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
                    UpdateBeginTime(roadSegmentRecord, envelope);
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
        WfsContext context,
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
        WfsContext context,
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

    private static RoadSegmentRecord UpdateBeginTime(RoadSegmentRecord record, Envelope<RoadNetworkChangesAccepted> envelope)
    {
        record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        return record;
    }
}
