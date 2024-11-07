namespace RoadRegistry.Wms.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.FeatureToggles;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;

public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
{
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentRecordProjection(IStreetNameCache streetNameCache, UseRoadSegmentSoftDeleteFeatureToggle useRoadSegmentSoftDeleteFeatureToggle)
    {
        _streetNameCache = streetNameCache.ThrowIfNull();

        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
            var status = RoadSegmentStatus.Parse(envelope.Message.Status);
            var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
            var category = RoadSegmentCategory.Parse(envelope.Message.Category);
            var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

            var leftSideStreetNameRecord = await TryGetFromStreetNameCache(envelope.Message.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromStreetNameCache(envelope.Message.RightSide.StreetNameId, token);

            await context.RoadSegments.AddAsync(new RoadSegmentRecord
            {
                Id = envelope.Message.Id,
                BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                BeginOrganizationName = envelope.Message.Origin.Organization,
                BeginTime = envelope.Message.Origin.Since,
                BeginApplication = envelope.Message.Origin.Application,

                MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name),

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
            foreach (var acceptedChange in envelope.Message.Changes.Flatten())
                switch (acceptedChange)
                {
                    case RoadSegmentAdded roadSegmentAdded:
                        await AddRoadSegment(context, envelope, roadSegmentAdded, token);
                        break;

                    case RoadSegmentModified roadSegmentModified:
                        await ModifyRoadSegment(context, envelope, roadSegmentModified, token);
                        break;

                    case RoadSegmentAddedToEuropeanRoad change:
                        await AddRoadSegmentToEuropeanRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad change:
                        await RemoveRoadSegmentFromEuropeanRoad(context, change, envelope, token);
                        break;

                    case RoadSegmentAddedToNationalRoad change:
                        await AddRoadSegmentToNationalRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad change:
                        await RemoveRoadSegmentFromNationalRoad(context, change, envelope, token);
                        break;

                    case RoadSegmentAddedToNumberedRoad change:
                        await AddRoadSegmentToNumberedRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad change:
                        await RemoveRoadSegmentFromNumberedRoad(context, change, envelope, token);
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

    private async Task AddRoadSegment(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAdded roadSegmentAdded,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentRecord
            {
                Id = roadSegmentAdded.Id
            };
            await context.RoadSegments.AddAsync(dbRecord, token);
        }
        else
        {
            dbRecord.IsRemoved = false;
        }

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

        var leftSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentAdded.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentAdded.RightSide.StreetNameId, token);

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.BeginOrganizationId = envelope.Message.OrganizationId;
        dbRecord.BeginOrganizationName = envelope.Message.Organization;

        dbRecord.BeginApplication = null;

        dbRecord.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);

        dbRecord.MethodId = method.Translation.Identifier;
        dbRecord.MethodDutchName = method.Translation.Name;

        dbRecord.CategoryId = category.Translation.Identifier;
        dbRecord.CategoryDutchName = category.Translation.Name;

        dbRecord.Geometry2D = WmsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry);
        dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;

        dbRecord.MorphologyId = morphology.Translation.Identifier;
        dbRecord.MorphologyDutchName = morphology.Translation.Name;

        dbRecord.StatusId = status.Translation.Identifier;
        dbRecord.StatusDutchName = status.Translation.Name;

        dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
        dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

        dbRecord.RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

        dbRecord.LeftSideMunicipalityId = null;
        dbRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
        dbRecord.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

        dbRecord.RightSideMunicipalityId = null;
        dbRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
        dbRecord.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

        dbRecord.RoadSegmentVersion = roadSegmentAdded.Version;

        dbRecord.BeginRoadNodeId = roadSegmentAdded.StartNodeId;
        dbRecord.EndRoadNodeId = roadSegmentAdded.EndNodeId;
    }

    private async Task ModifyRoadSegment(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentModified roadSegmentModified,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentRecord
            {
                Id = roadSegmentModified.Id
            };
            await context.RoadSegments.AddAsync(dbRecord, token);
        }

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);
        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

        var leftSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentModified.LeftSide.StreetNameId, token);
        var rightSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentModified.RightSide.StreetNameId, token);

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbRecord.Id = roadSegmentModified.Id;
        dbRecord.BeginApplication = null;

        dbRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);

        dbRecord.MethodId = method.Translation.Identifier;
        dbRecord.MethodDutchName = method.Translation.Name;

        dbRecord.CategoryId = category.Translation.Identifier;
        dbRecord.CategoryDutchName = category.Translation.Name;

        dbRecord.Geometry2D = WmsGeometryTranslator.Translate2D(roadSegmentModified.Geometry);
        dbRecord.GeometryVersion = roadSegmentModified.GeometryVersion;

        dbRecord.MorphologyId = morphology.Translation.Identifier;
        dbRecord.MorphologyDutchName = morphology.Translation.Name;

        dbRecord.StatusId = status.Translation.Identifier;
        dbRecord.StatusDutchName = status.Translation.Name;

        dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
        dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

        dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

        dbRecord.LeftSideMunicipalityId = null;
        dbRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
        dbRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

        dbRecord.RightSideMunicipalityId = null;
        dbRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
        dbRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

        dbRecord.RoadSegmentVersion = roadSegmentModified.Version;

        dbRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
        dbRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
    }

    private static async Task AddRoadSegmentToEuropeanRoad(
        WmsContext context,
        RoadSegmentAddedToEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromEuropeanRoad(
        WmsContext context,
        RoadSegmentRemovedFromEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNationalRoad(
        WmsContext context,
        RoadSegmentAddedToNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNationalRoad(
        WmsContext context,
        RoadSegmentRemovedFromNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNumberedRoad(
        WmsContext context,
        RoadSegmentAddedToNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNumberedRoad(
        WmsContext context,
        RoadSegmentRemovedFromNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private async Task ModifyRoadSegmentAttributes(
        WmsContext context,
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

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

            dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;
        }

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

            dbRecord.StatusId = status.Translation.Identifier;
            dbRecord.StatusDutchName = status.Translation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

            dbRecord.MorphologyId = morphology.Translation.Identifier;
            dbRecord.MorphologyDutchName = morphology.Translation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

            dbRecord.CategoryId = category.Translation.Identifier;
            dbRecord.CategoryDutchName = category.Translation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            dbRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
        }

        if (roadSegmentAttributesModified.LeftSide is not null)
        {
            var streetNameRecord = await TryGetFromStreetNameCache(roadSegmentAttributesModified.LeftSide.StreetNameId, token);

            dbRecord.LeftSideMunicipalityId = null;
            dbRecord.LeftSideMunicipalityNisCode = streetNameRecord?.NisCode;
            dbRecord.LeftSideStreetNameId = roadSegmentAttributesModified.LeftSide.StreetNameId;
            dbRecord.LeftSideStreetName = streetNameRecord?.Name;
        }

        if (roadSegmentAttributesModified.RightSide is not null)
        {
            var streetNameRecord = await TryGetFromStreetNameCache(roadSegmentAttributesModified.RightSide.StreetNameId, token);

            dbRecord.RightSideMunicipalityId = null;
            dbRecord.RightSideMunicipalityNisCode = streetNameRecord?.NisCode;
            dbRecord.RightSideStreetNameId = roadSegmentAttributesModified.RightSide.StreetNameId;
            dbRecord.RightSideStreetName = streetNameRecord?.Name;
        }

        dbRecord.RoadSegmentVersion = roadSegmentAttributesModified.Version;

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();
    }

    private static async Task ModifyRoadSegmentGeometry(
        WmsContext context,
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

        dbRecord.Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry);
        dbRecord.GeometryVersion = segment.GeometryVersion;

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.RoadSegmentVersion = segment.Version;

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();
    }

    private static async Task RemoveRoadSegment(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentRemoved roadSegmentRemoved,
        bool softDelete,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentRemoved.Id, token)
            .ConfigureAwait(false);
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

    private static async Task UpdateRoadSegmentVersion(
        WmsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int segmentId,
        int? segmentVersion,
        CancellationToken token)
    {
        if (segmentVersion is null)
        {
            return;
        }

        var dbRecord = await context.RoadSegments
                    .IncludeLocalSingleOrDefaultAsync(x => x.Id == segmentId, token)
                    .ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {segmentId} is not found");
        }

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        var transactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

        dbRecord.RoadSegmentVersion = segmentVersion.Value;
    }

    private async Task RenameOrganization(
        WmsContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        await context.RoadSegments
            .IncludeLocalForEachBatchAsync(q => q
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
        await context.RoadSegments.IncludeLocalForEachBatchAsync(q =>
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

    private async Task<StreetNameCacheItem> TryGetFromStreetNameCache(
        int? streetNameId,
        CancellationToken token)
    {
        return streetNameId.HasValue ? await _streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) : null;
    }
}
