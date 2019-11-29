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
    using Schema.RoadSegmentNationalRoadAttributes;

    public class RoadSegmentNationalRoadAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadSegmentNationalRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
            {
                if (envelope.Message.PartOfNationalRoads.Length == 0)
                    return Task.CompletedTask;


                var nationalRoadAttributes = envelope.Message
                    .PartOfNationalRoads
                    .Select(nationalRoad => new RoadSegmentNationalRoadAttributeRecord
                    {
                        Id = nationalRoad.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                        {
                            NW_OIDN = {Value = nationalRoad.AttributeId},
                            WS_OIDN = {Value = envelope.Message.Id},
                            IDENT2 = {Value = nationalRoad.Ident2},
                            BEGINTIJD = {Value = nationalRoad.Origin.Since},
                            BEGINORG = {Value = nationalRoad.Origin.OrganizationId},
                            LBLBGNORG = {Value = nationalRoad.Origin.Organization},
                        }.ToBytes(manager, encoding)
                    });

                return context.AddRangeAsync(nationalRoadAttributes, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>((context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAddedToNationalRoad nationalRoad:
                            context.RoadSegmentNationalRoadAttributes.Add(new RoadSegmentNationalRoadAttributeRecord
                            {
                                Id = nationalRoad.AttributeId,
                                RoadSegmentId = nationalRoad.SegmentId,
                                DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                                {
                                    NW_OIDN = {Value = nationalRoad.AttributeId},
                                    WS_OIDN = {Value = nationalRoad.SegmentId},
                                    IDENT2 = {Value = nationalRoad.Ident2},
                                    // TODO: Needs to come from the event
                                    BEGINTIJD = {Value = null},
                                    BEGINORG = {Value = null},
                                    LBLBGNORG = {Value = null},
                                }.ToBytes(manager, encoding)
                            });
                            break;
                    }
                }

                return Task.CompletedTask;
            });
        }
    }
}
