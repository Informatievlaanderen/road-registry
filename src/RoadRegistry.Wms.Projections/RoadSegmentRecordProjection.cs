namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Text;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadSegmentDenorm;

    public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
    {
        public RoadSegmentRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {

                await context.RoadSegments.AddAsync(new RoadSegmentDenormRecord()
                {
                    Id = envelope.Message.Id,
                }, token);
            });
        }
    }
}
