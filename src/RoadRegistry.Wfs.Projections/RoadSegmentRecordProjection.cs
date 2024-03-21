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
                MaintainerName = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name),
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
        var dbRecord = new RoadSegmentRecord
        {
            Id = roadSegmentAdded.Id
        };
        await context.RoadSegments.AddAsync(dbRecord, token);

        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

        var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.RightSide.StreetNameId, token);
        
        dbRecord.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);
        dbRecord.MethodDutchName = method.Translation.Name;
        dbRecord.CategoryDutchName = category.Translation.Name;
        dbRecord.Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry);
        dbRecord.MorphologyDutchName = morphology.Translation.Name;
        dbRecord.StatusDutchName = status.Translation.Name;
        dbRecord.AccessRestriction = accessRestriction.Translation.Name;
        dbRecord.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;
        dbRecord.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;
        dbRecord.BeginRoadNodeId = roadSegmentAdded.StartNodeId;
        dbRecord.EndRoadNodeId = roadSegmentAdded.EndNodeId;

        UpdateBeginTime(dbRecord, envelope);
    }

    private static async Task ModifyRoadSegment(IStreetNameCache streetNameCache,
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentModified roadSegmentModified,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found");
        }

        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);
        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

        var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.RightSide.StreetNameId, token);
        
        dbRecord.Id = roadSegmentModified.Id;
        dbRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);
        dbRecord.MethodDutchName = method.Translation.Name;
        dbRecord.CategoryDutchName = category.Translation.Name;
        dbRecord.Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentModified.Geometry);
        dbRecord.MorphologyDutchName = morphology.Translation.Name;
        dbRecord.StatusDutchName = status.Translation.Name;
        dbRecord.AccessRestriction = accessRestriction.Translation.Name;
        dbRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;
        dbRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;
        dbRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
        dbRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;

        UpdateBeginTime(dbRecord, envelope);
    }

    private static async Task ModifyRoadSegmentAttributes(
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAttributesModified.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

            dbRecord.AccessRestriction = accessRestriction.Translation.Name;
        }

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

            dbRecord.StatusDutchName = status.Translation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

            dbRecord.MorphologyDutchName = morphology.Translation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

            dbRecord.CategoryDutchName = category.Translation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            dbRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
        }

        UpdateBeginTime(dbRecord, envelope);
    }

    private static async Task ModifyRoadSegmentGeometry(
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentGeometryModified segment,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == segment.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
        }

        dbRecord.Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry);

        UpdateBeginTime(dbRecord, envelope);
    }

    private static async Task RemoveRoadSegment(
        WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemoved roadSegmentRemoved,
        bool softDelete,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentRemoved.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is not null)
        {
            if (softDelete)
            {
                if (!dbRecord.IsRemoved)
                {
                    UpdateBeginTime(dbRecord, envelope);
                    dbRecord.IsRemoved = true;
                }
            }
            else
            {
                context.RoadSegments.Remove(dbRecord);
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
