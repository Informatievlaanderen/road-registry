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
    using Schema.RoadSegmentEuropeanRoadAttributes;

    public class RoadSegmentEuropeanRoadAttributeRecordProjection: ConnectedProjection<ShapeContext>
    {
        public RoadSegmentEuropeanRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
            {
                if (envelope.Message.PartOfEuropeanRoads.Length == 0)
                    return Task.CompletedTask;

                var europeanRoadAttributes = envelope.Message
                    .PartOfEuropeanRoads
                    .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                    {
                        Id = europeanRoad.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                        {
                            EU_OIDN = {Value = europeanRoad.AttributeId},
                            WS_OIDN = {Value = envelope.Message.Id},
                            EUNUMMER = {Value = europeanRoad.Number},
                            BEGINTIJD = {Value = europeanRoad.Origin.Since},
                            BEGINORG = {Value = europeanRoad.Origin.OrganizationId},
                            LBLBGNORG = {Value = europeanRoad.Origin.Organization},
                        }.ToBytes(manager, encoding)
                    });

                return context.AddRangeAsync(europeanRoadAttributes, token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAddedToEuropeanRoad europeanRoad:
                            await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeRecord
                            {
                                Id = europeanRoad.AttributeId,
                                RoadSegmentId = europeanRoad.SegmentId,
                                DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                                {
                                    EU_OIDN = {Value = europeanRoad.AttributeId},
                                    WS_OIDN = {Value = europeanRoad.SegmentId},
                                    EUNUMMER = {Value = europeanRoad.Number},
                                    // TODO: Needs to come from the event
                                    BEGINTIJD = {Value = null},
                                    BEGINORG = {Value = null},
                                    LBLBGNORG = {Value = null},
                                }.ToBytes(manager, encoding)
                            });
                            break;
                    }
                }
            });
        }
    }
}
