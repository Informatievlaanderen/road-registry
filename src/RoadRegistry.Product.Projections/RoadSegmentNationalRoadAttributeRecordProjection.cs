namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadSegments;

    public class RoadSegmentNationalRoadAttributeRecordProjection : ConnectedProjection<ProductContext>
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
                            IDENT2 = {Value = nationalRoad.Number},
                            BEGINTIJD = {Value = nationalRoad.Origin.Since},
                            BEGINORG = {Value = nationalRoad.Origin.OrganizationId},
                            LBLBGNORG = {Value = nationalRoad.Origin.Organization},
                        }.ToBytes(manager, encoding)
                    });

                return context.RoadSegmentNationalRoadAttributes.AddRangeAsync(nationalRoadAttributes, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAddedToNationalRoad nationalRoad:
                            await context.RoadSegmentNationalRoadAttributes.AddAsync(new RoadSegmentNationalRoadAttributeRecord
                            {
                                Id = nationalRoad.AttributeId,
                                RoadSegmentId = nationalRoad.SegmentId,
                                DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                                {
                                    NW_OIDN = {Value = nationalRoad.AttributeId},
                                    WS_OIDN = {Value = nationalRoad.SegmentId},
                                    IDENT2 = {Value = nationalRoad.Number},
                                    BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                    BEGINORG = {Value = envelope.Message.OrganizationId},
                                    LBLBGNORG = {Value = envelope.Message.Organization}
                                }.ToBytes(manager, encoding)
                            });
                            break;
                    }
                }
            });
        }
    }
}
