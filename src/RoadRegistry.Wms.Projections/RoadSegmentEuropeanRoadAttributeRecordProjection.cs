namespace RoadRegistry.Wms.Projections;

using System.Linq;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;

public class RoadSegmentEuropeanRoadAttributeRecordProjection : ConnectedProjection<WmsContext>
{
    public RoadSegmentEuropeanRoadAttributeRecordProjection()
    {
        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            if (envelope.Message.PartOfEuropeanRoads.Length > 0)
            {
                var europeanRoadAttributes = envelope.Message
                    .PartOfEuropeanRoads
                    .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                    {
                        WS_OIDN = envelope.Message.Id,
                        EU_OIDN = europeanRoad.AttributeId,
                        EUNUMMER = europeanRoad.Number,
                        BEGINTIJD = europeanRoad.Origin.Since,
                        BEGINORG = europeanRoad.Origin.OrganizationId,
                        LBLBGNORG = europeanRoad.Origin.Organization
                    });

                await context.RoadSegmentEuropeanRoadAttributes.AddRangeAsync(europeanRoadAttributes, token);
            }
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
            {
                switch (change)
                {
                    case RoadSegmentAddedToEuropeanRoad europeanRoad:
                        await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeRecord
                        {
                            WS_OIDN = europeanRoad.SegmentId,
                            EU_OIDN = europeanRoad.AttributeId,
                            EUNUMMER = europeanRoad.Number,
                            BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                            BEGINORG = envelope.Message.OrganizationId,
                            LBLBGNORG = envelope.Message.Organization
                        });
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
                        var roadSegmentEuropeanRoadAttributeRecord =
                            await context.RoadSegmentEuropeanRoadAttributes.FindAsync(europeanRoad.AttributeId);

                        if (roadSegmentEuropeanRoadAttributeRecord != null)
                        {
                            context.RoadSegmentEuropeanRoadAttributes.Remove(roadSegmentEuropeanRoadAttributeRecord);
                        }

                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        var roadSegmentNationalRoadAttributeRecords =
                            context.RoadSegmentNationalRoadAttributes
                                .Local
                                .Where(x => x.WS_OIDN == roadSegmentRemoved.Id)
                                .Concat(context.RoadSegmentNationalRoadAttributes
                                    .Where(x => x.WS_OIDN == roadSegmentRemoved.Id));

                        context.RoadSegmentNationalRoadAttributes.RemoveRange(roadSegmentNationalRoadAttributeRecords);
                        break;
                }
            }
        });
    }
}
