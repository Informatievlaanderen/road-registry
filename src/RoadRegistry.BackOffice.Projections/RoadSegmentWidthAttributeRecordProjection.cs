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
    using Schema;
    using Schema.RoadSegmentWidthAttributes;

    public class RoadSegmentWidthAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadSegmentWidthAttributeRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
            {
                if (envelope.Message.Widths.Length == 0)
                    return Task.CompletedTask;

                var widths = envelope.Message
                    .Widths
                    .Select(width => new RoadSegmentWidthAttributeRecord
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                        {
                            WB_OIDN = {Value = width.AttributeId},
                            WS_OIDN = {Value = envelope.Message.Id},
                            WS_GIDN = {Value = $"{envelope.Message.Id}_{width.AsOfGeometryVersion}"},
                            BREEDTE = {Value = width.Width},
                            VANPOS = {Value = (double) width.FromPosition},
                            TOTPOS = {Value = (double) width.ToPosition},
                            BEGINTIJD = {Value = width.Origin.Since},
                            BEGINORG = {Value = width.Origin.OrganizationId},
                            LBLBGNORG = {Value = width.Origin.Organization}
                        }.ToBytes(manager, encoding)
                    });

                return context.AddRangeAsync(widths, token);
            });
        }
    }
}
