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
    using Schema;
    using Schema.RoadSegmentWidthAttributes;

    public class RoadSegmentWidthAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;
        public RoadSegmentWidthAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.Widths.Length == 0)
                return Task.CompletedTask;

            var widths = @event
                .Widths
                .Select(width => new RoadSegmentWidthAttributeRecord
                {
                    Id = width.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                    {
                        WB_OIDN = { Value = width.AttributeId },
                        WS_OIDN = { Value = @event.Id },
                        WS_GIDN = { Value = $"{@event.Id}_{width.AsOfGeometryVersion}" },
                        BREEDTE = { Value = width.Width },
                        VANPOS = { Value = (double)width.FromPosition },
                        TOTPOS = { Value = (double)width.ToPosition },
                        BEGINTIJD = { Value = width.Origin.Since },
                        BEGINORG = { Value = width.Origin.OrganizationId },
                        LBLBGNORG = { Value = width.Origin.Organization }
                    }.ToBytes(_encoding)
                });

            return context.AddRangeAsync(widths, token);
        }
    }
}
