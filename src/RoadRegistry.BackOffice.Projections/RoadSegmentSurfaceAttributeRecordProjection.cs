namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Model;
    using Schema;
    using Schema.RoadSegmentSurfaceAttributes;

    public class RoadSegmentSurfaceAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;

        public RoadSegmentSurfaceAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.Surfaces.Length == 0)
                return Task.CompletedTask;

            var surfaces = @event
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;
                    return new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = @event.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = {Value = surface.AttributeId},
                            WS_OIDN = {Value = @event.Id},
                            WS_GIDN = {Value = $"{@event.Id}_{surface.AsOfGeometryVersion}"},
                            TYPE = {Value = typeTranslation.Identifier},
                            LBLTYPE = {Value = typeTranslation.Name},
                            VANPOS = {Value = (double) surface.FromPosition},
                            TOTPOS = {Value = (double) surface.ToPosition},
                            BEGINTIJD = {Value = surface.Origin.Since},
                            BEGINORG = {Value = surface.Origin.OrganizationId},
                            LBLBGNORG = {Value = surface.Origin.Organization},
                        }.ToBytes(_encoding)
                    };
                });

            return context.AddRangeAsync(surfaces, token);
        }
    }
}
