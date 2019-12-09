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
    using Schema.RoadSegmentNumberedRoadAttributes;

    public class RoadSegmentNumberedRoadAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadSegmentNumberedRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
            {
                if (envelope.Message.PartOfNumberedRoads.Length == 0)
                    return Task.CompletedTask;

                var numberedRoadAttributes = envelope.Message
                    .PartOfNumberedRoads
                    .Select(numberedRoad =>
                    {
                        var directionTranslation =
                            RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
                        return new RoadSegmentNumberedRoadAttributeRecord
                        {
                            Id = numberedRoad.AttributeId,
                            RoadSegmentId = envelope.Message.Id,
                            DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                            {
                                GW_OIDN = {Value = numberedRoad.AttributeId},
                                WS_OIDN = {Value = envelope.Message.Id},
                                IDENT8 = {Value = numberedRoad.Ident8},
                                RICHTING = {Value = directionTranslation.Identifier},
                                LBLRICHT = {Value = directionTranslation.Name},
                                VOLGNUMMER = {Value = numberedRoad.Ordinal},
                                BEGINTIJD = {Value = numberedRoad.Origin.Since},
                                BEGINORG = {Value = numberedRoad.Origin.OrganizationId},
                                LBLBGNORG = {Value = numberedRoad.Origin.Organization},
                            }.ToBytes(manager, encoding)
                        };
                    });
                return context.RoadSegmentNumberedRoadAttributes.AddRangeAsync(numberedRoadAttributes, token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAddedToNumberedRoad numberedRoad:
                            var directionTranslation =
                                RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
                            await context.RoadSegmentNumberedRoadAttributes.AddAsync(new RoadSegmentNumberedRoadAttributeRecord
                            {
                                Id = numberedRoad.AttributeId,
                                RoadSegmentId = numberedRoad.SegmentId,
                                DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                                {
                                    GW_OIDN = {Value = numberedRoad.AttributeId},
                                    WS_OIDN = {Value = numberedRoad.SegmentId},
                                    IDENT8 = {Value = numberedRoad.Ident8},
                                    RICHTING = {Value = directionTranslation.Identifier},
                                    LBLRICHT = {Value = directionTranslation.Name},
                                    VOLGNUMMER = {Value = numberedRoad.Ordinal},
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
