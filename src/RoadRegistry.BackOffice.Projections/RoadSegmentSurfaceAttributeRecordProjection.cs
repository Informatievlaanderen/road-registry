namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Microsoft.IO;
    using Model;
    using Schema;
    using Schema.RoadSegmentSurfaceAttributes;

    public class RoadSegmentSurfaceAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadSegmentSurfaceAttributeRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
            {
                if (envelope.Message.Surfaces.Length == 0)
                    return Task.CompletedTask;

                var surfaces = envelope.Message
                    .Surfaces
                    .Select(surface =>
                    {
                        var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;
                        return new RoadSegmentSurfaceAttributeRecord
                        {
                            Id = surface.AttributeId,
                            RoadSegmentId = envelope.Message.Id,
                            DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                            {
                                WV_OIDN = {Value = surface.AttributeId},
                                WS_OIDN = {Value = envelope.Message.Id},
                                WS_GIDN = {Value = $"{envelope.Message.Id}_{surface.AsOfGeometryVersion}"},
                                TYPE = {Value = typeTranslation.Identifier},
                                LBLTYPE = {Value = typeTranslation.Name},
                                VANPOS = {Value = (double) surface.FromPosition},
                                TOTPOS = {Value = (double) surface.ToPosition},
                                BEGINTIJD = {Value = surface.Origin.Since},
                                BEGINORG = {Value = surface.Origin.OrganizationId},
                                LBLBGNORG = {Value = surface.Origin.Organization},
                            }.ToBytes(manager, encoding)
                        };
                    });

                return context.AddRangeAsync(surfaces, token);
            });
        }
    }
}
