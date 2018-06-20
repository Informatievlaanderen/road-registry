namespace RoadRegistry.Projections
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentDynamicWidthAttributeProjection : ConnectedProjection<ShapeContext>
    {
        public RoadSegmentDynamicWidthAttributeProjection()
        {
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(null == @event?.Widths || @event.Widths.Length == 0)
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
                        WS_GIDN = { Value = $"{@event.Id}_{@event.GeometryVersion}" },
                        BREEDTE = { Value = width.Width },
                        VANPOS = { Value = (double)width.FromPosition },
                        TOTPOS = { Value = (double)width.ToPosition },
                        BEGINTIJD = { Value = width.Origin.Since },
                        BEGINORG = { Value = width.Origin.OrganizationId },
                        LBLBGNORG = { Value = width.Origin.Organization }
                    }.ToBytes()
                });

            return context.AddRangeAsync(widths, token);
        }
    }
}
