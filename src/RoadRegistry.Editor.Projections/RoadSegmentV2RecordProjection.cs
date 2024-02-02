namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;
using Schema;
using Schema.Extensions;
using Schema.RoadSegments;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadSegmentV2RecordProjection : ConnectedProjection<EditorContext>
{
    public RoadSegmentV2RecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var statusTranslation = RoadSegmentStatus.Parse(envelope.Message.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(envelope.Message.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(envelope.Message.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction).Translation;
            var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

            var dbaseRecord = new RoadSegmentDbaseRecord
            {
                WS_OIDN = { Value = envelope.Message.Id },
                WS_UIDN = { Value = new UIDN(envelope.Message.Id, envelope.Message.Version) },
                WS_GIDN = { Value = new UIDN(envelope.Message.Id, envelope.Message.GeometryVersion) },
                B_WK_OIDN = { Value = envelope.Message.StartNodeId },
                E_WK_OIDN = { Value = envelope.Message.EndNodeId },
                STATUS = { Value = statusTranslation.Identifier },
                LBLSTATUS = { Value = statusTranslation.Name },
                MORF = { Value = morphologyTranslation.Identifier },
                LBLMORF = { Value = morphologyTranslation.Name },
                WEGCAT = { Value = categoryTranslation.Identifier },
                LBLWEGCAT = { Value = categoryTranslation.Name },
                LSTRNMID = { Value = envelope.Message.LeftSide.StreetNameId },
                LSTRNM = { Value = envelope.Message.LeftSide.StreetName },
                RSTRNMID = { Value = envelope.Message.RightSide.StreetNameId },
                RSTRNM = { Value = envelope.Message.RightSide.StreetName },
                BEHEER = { Value = envelope.Message.MaintenanceAuthority.Code },
                LBLBEHEER = { Value = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name) },
                METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                OPNDATUM = { Value = envelope.Message.RecordingDate },
                BEGINTIJD = { Value = envelope.Message.Origin.Since },
                BEGINORG = { Value = envelope.Message.Origin.OrganizationId },
                LBLBGNORG = { Value = envelope.Message.Origin.Organization },
                TGBEP = { Value = accessRestrictionTranslation.Identifier },
                LBLTGBEP = { Value = accessRestrictionTranslation.Name }
            };

            await context.RoadSegmentsV2.AddAsync(
                UpdateHash(new RoadSegmentV2Record
                {
                    Id = envelope.Message.Id,
                    StartNodeId = envelope.Message.StartNodeId,
                    EndNodeId = envelope.Message.EndNodeId,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),
                    
                    Version = envelope.Message.Version,
                    GeometryVersion = envelope.Message.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                    MaintainerName = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name),
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    TransactionId = transactionId,
                    RecordingDate = envelope.Message.RecordingDate,
                    BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                    BeginOrganizationName = envelope.Message.Origin.Organization,

                    DbaseRecord = dbaseRecord.ToBytes(manager, encoding)
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape)), envelope.Message),
                token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var message in envelope.Message.Changes.Flatten())
                switch (message)
                {
                    case RoadSegmentAdded roadSegmentAdded:
                        await AddRoadSegment(manager, encoding, context, roadSegmentAdded, envelope, token);
                        break;

                    case RoadSegmentModified roadSegmentModified:
                        await ModifyRoadSegment(manager, encoding, context, roadSegmentModified, envelope, token);
                        break;

                    case RoadSegmentAttributesModified roadSegmentAttributesModified:
                        await ModifyRoadSegmentAttributes(manager, encoding, context, roadSegmentAttributesModified, envelope, token);
                        break;

                    case RoadSegmentGeometryModified roadSegmentGeometryModified:
                        await ModifyRoadSegmentGeometry(manager, encoding, context, roadSegmentGeometryModified, envelope, token);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RemoveRoadSegment(manager, encoding, context, roadSegmentRemoved, envelope, token);
                        break;
                }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await RenameOrganization(manager, encoding, context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            if (envelope.Message.NameModified)
            {
                await RenameOrganization(manager, encoding, context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
            }
        });
    }

    private static async Task AddRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentsV2
            .FindWithoutQueryFiltersAsync(x => x.Id == roadSegmentAdded.Id, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentV2Record
            {
                Id = roadSegmentAdded.Id
            };
            await context.RoadSegmentsV2.AddAsync(dbRecord, token);
        }
        else
        {
            dbRecord.IsRemoved = false;
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        dbRecord.StartNodeId = roadSegmentAdded.StartNodeId;
        dbRecord.EndNodeId = roadSegmentAdded.EndNodeId;
        dbRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        dbRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        dbRecord.Version = roadSegmentAdded.Version;
        dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;
        dbRecord.StatusId = statusTranslation.Identifier;
        dbRecord.MorphologyId = morphologyTranslation.Identifier;
        dbRecord.CategoryId = categoryTranslation.Identifier;
        dbRecord.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        dbRecord.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        dbRecord.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);
        dbRecord.MethodId = geometryDrawMethodTranslation.Identifier;
        dbRecord.AccessRestrictionId = accessRestrictionTranslation.Identifier;

        dbRecord.TransactionId = transactionId;
        dbRecord.RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbRecord.BeginOrganizationId = envelope.Message.OrganizationId;
        dbRecord.BeginOrganizationName = envelope.Message.Organization;

        var dbaseRecord = new RoadSegmentDbaseRecord();
        if (dbRecord.DbaseRecord is not null)
        {
            dbaseRecord.FromBytes(dbRecord.DbaseRecord, manager, encoding);
        }

        dbaseRecord.BEGINORG.Value = envelope.Message.OrganizationId;
        dbaseRecord.LBLBGNORG.Value = envelope.Message.Organization;
        dbaseRecord.WS_OIDN.Value = roadSegmentAdded.Id;
        dbaseRecord.OPNDATUM.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbaseRecord.WS_UIDN.Value = new UIDN(roadSegmentAdded.Id, roadSegmentAdded.Version);
        dbaseRecord.WS_GIDN.Value = new UIDN(roadSegmentAdded.Id, roadSegmentAdded.GeometryVersion);
        dbaseRecord.B_WK_OIDN.Value = roadSegmentAdded.StartNodeId;
        dbaseRecord.E_WK_OIDN.Value = roadSegmentAdded.EndNodeId;
        dbaseRecord.STATUS.Value = statusTranslation.Identifier;
        dbaseRecord.LBLSTATUS.Value = statusTranslation.Name;
        dbaseRecord.MORF.Value = morphologyTranslation.Identifier;
        dbaseRecord.LBLMORF.Value = morphologyTranslation.Name;
        dbaseRecord.WEGCAT.Value = categoryTranslation.Identifier;
        dbaseRecord.LBLWEGCAT.Value = categoryTranslation.Name;
        dbaseRecord.LSTRNMID.Value = roadSegmentAdded.LeftSide.StreetNameId;
        dbaseRecord.LSTRNM.Value = null; // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
        dbaseRecord.RSTRNMID.Value = roadSegmentAdded.RightSide.StreetNameId;
        dbaseRecord.RSTRNM.Value = null; // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
        dbaseRecord.BEHEER.Value = roadSegmentAdded.MaintenanceAuthority.Code;
        dbaseRecord.LBLBEHEER.Value = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);
        dbaseRecord.METHODE.Value = geometryDrawMethodTranslation.Identifier;
        dbaseRecord.LBLMETHOD.Value = geometryDrawMethodTranslation.Name;
        dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
        dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);

        UpdateHash(dbRecord, roadSegmentAdded);
    }

    private static async Task ModifyRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentsV2.FindAsync(x => x.Id == roadSegmentModified.Id, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentV2Record)} with id {roadSegmentModified.Id} is not found");
        }

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentModified.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentModified.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction).Translation;
        var transactionId = new TransactionId(envelope.Message.TransactionId);

        dbRecord.Id = roadSegmentModified.Id;
        dbRecord.StartNodeId = roadSegmentModified.StartNodeId;
        dbRecord.EndNodeId = roadSegmentModified.EndNodeId;
        dbRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        dbRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        
        dbRecord.Version = roadSegmentModified.Version;
        dbRecord.GeometryVersion = roadSegmentModified.GeometryVersion;
        dbRecord.StatusId = statusTranslation.Identifier;
        dbRecord.MorphologyId = morphologyTranslation.Identifier;
        dbRecord.CategoryId = categoryTranslation.Identifier;
        dbRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        dbRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        dbRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);
        dbRecord.MethodId = geometryDrawMethodTranslation.Identifier;
        dbRecord.AccessRestrictionId = accessRestrictionTranslation.Identifier;

        dbRecord.TransactionId = transactionId;
        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        
        var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
        dbaseRecord.WS_UIDN.Value = new UIDN(roadSegmentModified.Id, roadSegmentModified.Version);
        dbaseRecord.WS_GIDN.Value = new UIDN(roadSegmentModified.Id, roadSegmentModified.GeometryVersion);
        dbaseRecord.B_WK_OIDN.Value = roadSegmentModified.StartNodeId;
        dbaseRecord.E_WK_OIDN.Value = roadSegmentModified.EndNodeId;
        dbaseRecord.STATUS.Value = statusTranslation.Identifier;
        dbaseRecord.LBLSTATUS.Value = statusTranslation.Name;
        dbaseRecord.MORF.Value = morphologyTranslation.Identifier;
        dbaseRecord.LBLMORF.Value = morphologyTranslation.Name;
        dbaseRecord.WEGCAT.Value = categoryTranslation.Identifier;
        dbaseRecord.LBLWEGCAT.Value = categoryTranslation.Name;
        dbaseRecord.LSTRNMID.Value = roadSegmentModified.LeftSide.StreetNameId;
        dbaseRecord.LSTRNM.Value = null; // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
        dbaseRecord.RSTRNMID.Value = roadSegmentModified.RightSide.StreetNameId;
        dbaseRecord.RSTRNM.Value = null; // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
        dbaseRecord.BEHEER.Value = roadSegmentModified.MaintenanceAuthority.Code;
        dbaseRecord.LBLBEHEER.Value = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);
        dbaseRecord.METHODE.Value = geometryDrawMethodTranslation.Identifier;
        dbaseRecord.LBLMETHOD.Value = geometryDrawMethodTranslation.Name;
        dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
        dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);

        UpdateHash(dbRecord, roadSegmentModified);
    }

    private static async Task ModifyRoadSegmentAttributes(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentsV2.FindAsync(x => x.Id == roadSegmentAttributesModified.Id, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentV2Record)} with id {roadSegmentAttributesModified.Id} is not found");
        }

        dbRecord.TransactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.Version = roadSegmentAttributesModified.Version;

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);

        if (roadSegmentAttributesModified.Status is not null)
        {
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status).Translation;

            dbRecord.StatusId = statusTranslation.Identifier;

            dbaseRecord.STATUS.Value = statusTranslation.Identifier;
            dbaseRecord.LBLSTATUS.Value = statusTranslation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology).Translation;

            dbRecord.MorphologyId = morphologyTranslation.Identifier;

            dbaseRecord.MORF.Value = morphologyTranslation.Identifier;
            dbaseRecord.LBLMORF.Value = morphologyTranslation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category).Translation;

            dbRecord.CategoryId = categoryTranslation.Identifier;

            dbaseRecord.WEGCAT.Value = categoryTranslation.Identifier;
            dbaseRecord.LBLWEGCAT.Value = categoryTranslation.Name;
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction).Translation;

            dbRecord.AccessRestrictionId = accessRestrictionTranslation.Identifier;

            dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
            dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            dbRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAttributesModified.MaintenanceAuthority.Name);

            dbaseRecord.BEHEER.Value = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbaseRecord.LBLBEHEER.Value = OrganizationName.FromValueWithFallback(roadSegmentAttributesModified.MaintenanceAuthority.Name);
        }

        dbaseRecord.WS_UIDN.Value = new UIDN(roadSegmentAttributesModified.Id, roadSegmentAttributesModified.Version);
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);

        UpdateHash(dbRecord, roadSegmentAttributesModified);
    }

    private static async Task ModifyRoadSegmentGeometry(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentsV2.FindAsync(x => x.Id == roadSegmentGeometryModified.Id, token).ConfigureAwait(false);
        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentV2Record)} with id {roadSegmentGeometryModified.Id} is not found");
        }
        
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);

        dbRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        dbRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        dbRecord.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
        dbRecord.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        dbRecord.TransactionId = new TransactionId(envelope.Message.TransactionId);
        dbRecord.Version = roadSegmentGeometryModified.Version;
        dbRecord.GeometryVersion = roadSegmentGeometryModified.GeometryVersion;

        dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        
        var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
        dbaseRecord.WS_UIDN.Value = new UIDN(roadSegmentGeometryModified.Id, roadSegmentGeometryModified.Version);
        dbaseRecord.WS_GIDN.Value = new UIDN(roadSegmentGeometryModified.Id, roadSegmentGeometryModified.GeometryVersion);
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);

        UpdateHash(dbRecord, roadSegmentGeometryModified);
    }

    private static async Task RemoveRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentRemoved roadSegmentRemoved,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentsV2.FindAsync(x => x.Id == roadSegmentRemoved.Id, token).ConfigureAwait(false);

        if (dbRecord is not null && !dbRecord.IsRemoved)
        {
            dbRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

            var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
            dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

            dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
            dbRecord.IsRemoved = true;

            UpdateHash(dbRecord, roadSegmentRemoved);
        }
    }

    private async Task RenameOrganization(
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        var organizationIdValue = organizationId.ToString();

        await context.RoadSegmentsV2
            .ForEachBatchAsync(q => q.Where(x => x.MaintainerId == organizationIdValue), 5000, dbRecords =>
            {
                foreach (var dbRecord in dbRecords)
                {
                    var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
                    var dataChanged = false;

                    if (dbaseRecord.BEHEER.Value == organizationId)
                    {
                        dbaseRecord.LBLBEHEER.Value = OrganizationName.FromValueWithFallback(organizationName);
                        dataChanged = true;
                    }

                    if (dataChanged)
                    {
                        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
                    }
                }

                return Task.CompletedTask;
            }, cancellationToken);
    }

    private static RoadSegmentV2Record UpdateHash<T>(RoadSegmentV2Record entity, T message) where T : IHaveHash
    {
        entity.LastEventHash = message.GetHash();
        return entity;
    }
}
