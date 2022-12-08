namespace RoadRegistry.Product.Projections;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.RoadSegments;
using Microsoft.IO;
using Schema;
using Schema.RoadSegments;
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
                        LBLBEHEER = { Value = envelope.Message.MaintenanceAuthority.Name },
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
                        await ModifyRoadSegment(manager, encoding, context, roadSegmentModified, envelope);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RemoveRoadSegment(context, roadSegmentRemoved);
                        break;
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
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
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
        Envelope<RoadNetworkChangesAccepted> envelope)
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

        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id);
        roadSegmentRecord.Id = roadSegmentModified.Id;
        roadSegmentRecord.ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding);
        roadSegmentRecord.ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32();
        roadSegmentRecord.BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape);
        var dbaseRecord = new RoadSegmentDbaseRecord();
        dbaseRecord.FromBytes(roadSegmentRecord.DbaseRecord, manager, encoding);
        // dbaseRecord.WS_OIDN.Value remains unchanged upon modification (it's the key)
        dbaseRecord.WS_UIDN.Value = $"{roadSegmentModified.Id}_{roadSegmentModified.Version}";
        dbaseRecord.WS_GIDN.Value = $"{roadSegmentModified.Id}_{roadSegmentModified.GeometryVersion}";
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
        dbaseRecord.LBLBEHEER.Value = roadSegmentModified.MaintenanceAuthority.Name;
        dbaseRecord.METHODE.Value = geometryDrawMethodTranslation.Identifier;
        dbaseRecord.LBLMETHOD.Value = geometryDrawMethodTranslation.Name;
        // dbaseRecord.OPNDATUM.Value remains unchanged upon modification
        dbaseRecord.BEGINTIJD.Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        dbaseRecord.BEGINORG.Value = envelope.Message.OrganizationId;
        dbaseRecord.LBLBGNORG.Value = envelope.Message.Organization;
        dbaseRecord.TGBEP.Value = accessRestrictionTranslation.Identifier;
        dbaseRecord.LBLTGBEP.Value = accessRestrictionTranslation.Name;
        roadSegmentRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
    }

    private static async Task RemoveRoadSegment(ProductContext context, RoadSegmentRemoved roadSegmentRemoved)
    {
        var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id);

        context.RoadSegments.Remove(roadSegmentRecord);
    }
}