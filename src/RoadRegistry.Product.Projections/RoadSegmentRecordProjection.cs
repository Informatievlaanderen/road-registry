namespace RoadRegistry.Product.Projections;

using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;
using Schema;
using Schema.RoadSegments;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadSegmentRecordProjection : ConnectedProjection<ProductContext>
{
    public RoadSegmentRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));

        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var geometry =
                GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var statusTranslation = RoadSegmentStatus.Parse(envelope.Message.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(envelope.Message.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(envelope.Message.Category).Translation;
            var geometryDrawMethodTranslation =
                RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation =
                RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction).Translation;
            await context.RoadSegments.AddAsync(
                new RoadSegmentRecord
                {
                    Id = envelope.Message.Id,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                    DbaseRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = envelope.Message.Id },
                        WS_UIDN = { Value = $"{envelope.Message.Id}_{envelope.Message.Version}" },
                        WS_GIDN = { Value = $"{envelope.Message.Id}_{envelope.Message.GeometryVersion}" },
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
                        LBLBEHEER = { Value = CleanOrganizationName(envelope.Message.MaintenanceAuthority.Name) },
                        METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                        LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                        OPNDATUM = { Value = envelope.Message.RecordingDate },
                        BEGINTIJD = { Value = envelope.Message.Origin.Since },
                        BEGINORG = { Value = envelope.Message.Origin.OrganizationId },
                        LBLBGNORG = { Value = envelope.Message.Origin.Organization },
                        TGBEP = { Value = accessRestrictionTranslation.Identifier },
                        LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                    }.ToBytes(manager, encoding)
                },
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
                        await RemoveRoadSegment(context, roadSegmentRemoved, token);
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
        ProductContext context,
        RoadSegmentAdded segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(segment.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;
        await context.RoadSegments.AddAsync(
            new RoadSegmentRecord
            {
                Id = segment.Id,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = $"{segment.Id}_{segment.Version}" },
                    WS_GIDN = { Value = $"{segment.Id}_{segment.GeometryVersion}" },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = statusTranslation.Identifier },
                    LBLSTATUS = { Value = statusTranslation.Name },
                    MORF = { Value = morphologyTranslation.Identifier },
                    LBLMORF = { Value = morphologyTranslation.Name },
                    WEGCAT = { Value = categoryTranslation.Identifier },
                    LBLWEGCAT = { Value = categoryTranslation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null }, // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null }, // This value is fetched from cache when downloading (see RoadSegmentsToZipArchiveWriter)
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = CleanOrganizationName(segment.MaintenanceAuthority.Name) },
                    METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                    BEGINORG = { Value = envelope.Message.OrganizationId },
                    LBLBGNORG = { Value = envelope.Message.Organization },
                    TGBEP = { Value = accessRestrictionTranslation.Identifier },
                    LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                }.ToBytes(manager, encoding)
            },
            token);
    }

    private static async Task ModifyRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentModified.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentModified.Category).Translation;
        var geometryDrawMethodTranslation =
            RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation =
            RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction).Translation;

        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found");
        }

        roadSegmentRecord.Id = roadSegmentModified.Id;
        roadSegmentRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        roadSegmentRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        roadSegmentRecord.BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape);
        var dbaseRecord = new RoadSegmentDbaseRecord();
        dbaseRecord.FromBytes(roadSegmentRecord.DbaseRecord, manager, encoding);
        // dbaseRecord.WS_OIDN.Value remains unchanged upon modification (it's the key)
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
        dbaseRecord.LBLBEHEER.Value = CleanOrganizationName(roadSegmentModified.MaintenanceAuthority.Name);
        dbaseRecord.METHODE.Value = geometryDrawMethodTranslation.Identifier;
        dbaseRecord.LBLMETHOD.Value = geometryDrawMethodTranslation.Name;
        // dbaseRecord.OPNDATUM.Value; //remains unchanged upon modification
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        // dbaseRecord.BEGINORG.Value = envelope.Message.OrganizationId; //remains unchanged upon modification
        // dbaseRecord.LBLBGNORG.Value = envelope.Message.Organization; //remains unchanged upon modification
        dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
        dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        roadSegmentRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
    }

    private static async Task ModifyRoadSegmentAttributes(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentAttributesModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
        }

        roadSegmentRecord.Id = roadSegmentAttributesModified.Id;
        var dbaseRecord = new RoadSegmentDbaseRecord();
        dbaseRecord.FromBytes(roadSegmentRecord.DbaseRecord, manager, encoding);

        if (roadSegmentAttributesModified.Status is not null)
        {
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status).Translation;

            dbaseRecord.STATUS.Value = statusTranslation.Identifier;
            dbaseRecord.LBLSTATUS.Value = statusTranslation.Name;
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology).Translation;

            dbaseRecord.MORF.Value = morphologyTranslation.Identifier;
            dbaseRecord.LBLMORF.Value = morphologyTranslation.Name;
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category).Translation;

            dbaseRecord.WEGCAT.Value = categoryTranslation.Identifier;
            dbaseRecord.LBLWEGCAT.Value = categoryTranslation.Name;
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestrictionTranslation =
                RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction).Translation;

            dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
            dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            dbaseRecord.BEHEER.Value = roadSegmentAttributesModified.MaintenanceAuthority.Code;
            dbaseRecord.LBLBEHEER.Value = CleanOrganizationName(roadSegmentAttributesModified.MaintenanceAuthority.Name);
        }

        // dbaseRecord.WS_OIDN.Value remains unchanged upon modification (it's the key)
        dbaseRecord.WS_UIDN.Value = new UIDN(roadSegmentAttributesModified.Id, roadSegmentAttributesModified.Version);
        // dbaseRecord.OPNDATUM.Value; //remains unchanged upon modification
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        // dbaseRecord.BEGINORG.Value = envelope.Message.OrganizationId; //remains unchanged upon modification
        // dbaseRecord.LBLBGNORG.Value = envelope.Message.Organization; //remains unchanged upon modification

        roadSegmentRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
    }

    private static async Task ModifyRoadSegmentGeometry(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(segment.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord == null)
        {
            throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
        }

        roadSegmentRecord.Id = segment.Id;
        var dbaseRecord = new RoadSegmentDbaseRecord();
        dbaseRecord.FromBytes(roadSegmentRecord.DbaseRecord, manager, encoding);

        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(segment.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);

        roadSegmentRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        roadSegmentRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        roadSegmentRecord.BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape);

        dbaseRecord.WS_GIDN.Value = new UIDN(segment.Id, segment.GeometryVersion);

        // dbaseRecord.WS_OIDN.Value remains unchanged upon modification (it's the key)
        dbaseRecord.WS_UIDN.Value = new UIDN(segment.Id, segment.Version);
        // dbaseRecord.OPNDATUM.Value; //remains unchanged upon modification
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        // dbaseRecord.BEGINORG.Value = envelope.Message.OrganizationId; //remains unchanged upon modification
        // dbaseRecord.LBLBGNORG.Value = envelope.Message.Organization; //remains unchanged upon modification

        roadSegmentRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
    }

    private static async Task RemoveRoadSegment(ProductContext context, RoadSegmentRemoved roadSegmentRemoved, CancellationToken token)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadSegmentRecord is not null)
        {
            context.RoadSegments.Remove(roadSegmentRecord);
        }
    }
    
    private async Task RenameOrganization(
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        await context.RoadSegments
            .ForEachBatchAsync(q => q, 5000, dbRecords =>
            {
                foreach (var dbRecord in dbRecords)
                {
                    var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
                    var dataChanged = false;

                    if (dbaseRecord.BEHEER.Value == organizationId)
                    {
                        dbaseRecord.LBLBEHEER.Value = CleanOrganizationName(organizationName);
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

    private static string CleanOrganizationName(string name)
    {
        return name.NullIfEmpty() ?? Organization.PredefinedTranslations.Unknown.Name;
    }
}
