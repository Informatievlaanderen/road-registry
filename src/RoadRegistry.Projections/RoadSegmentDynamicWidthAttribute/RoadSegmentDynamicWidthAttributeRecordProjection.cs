namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentDynamicWidthAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;
        public RoadSegmentDynamicWidthAttributeRecordProjection(Encoding encoding)
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
                .Select(width => new RoadSegmentDynamicWidthAttributeRecord
                {
                    Id = width.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentDynamicWidthAttributeDbaseRecord
                    {
                        WB_OIDN = { Value = width.AttributeId },
                        WS_OIDN = { Value = @event.Id },
                        WS_GIDN = { Value = $"{@event.Id}_{width.LastUpdatedForSegmentGeometryVersion}" },
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
