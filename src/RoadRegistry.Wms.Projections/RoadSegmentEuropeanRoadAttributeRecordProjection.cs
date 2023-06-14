namespace RoadRegistry.Wms.Projections;

using System.Linq;
using System.Threading.Tasks;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Schema;

public class RoadSegmentEuropeanRoadAttributeRecordProjection : ConnectedProjection<WmsContext>
{
    public RoadSegmentEuropeanRoadAttributeRecordProjection()
    {
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfEuropeanRoads.Length == 0)
            {
                return Task.CompletedTask;
            }

            var europeanRoadAttributes = envelope.Message
                .PartOfEuropeanRoads
                .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                {
                    EU_OIDN = europeanRoad.AttributeId,
                    WS_OIDN = envelope.Message.Id,
                    EUNUMMER = europeanRoad.Number,
                    BEGINTIJD = europeanRoad.Origin.Since,
                    BEGINORG = europeanRoad.Origin.OrganizationId,
                    LBLBGNORG = europeanRoad.Origin.Organization
                });
            
            return context.RoadSegmentEuropeanRoadAttributes.AddRangeAsync(europeanRoadAttributes, token);
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
                            EU_OIDN = europeanRoad.AttributeId,
                            WS_OIDN = europeanRoad.SegmentId,
                            EUNUMMER = europeanRoad.Number,
                            BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                            BEGINORG = envelope.Message.OrganizationId,
                            LBLBGNORG = envelope.Message.Organization
                        });
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
                        var roadSegmentEuropeanRoadAttributeRecord =
                            await context.RoadSegmentEuropeanRoadAttributes.FindAsync(europeanRoad.AttributeId, cancellationToken: token).ConfigureAwait(false);

                        if (roadSegmentEuropeanRoadAttributeRecord is not null)
                        {
                            context.RoadSegmentEuropeanRoadAttributes.Remove(roadSegmentEuropeanRoadAttributeRecord);
                        }

                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        var roadSegmentEuropeanRoadAttributeRecords =
                            context.RoadSegmentEuropeanRoadAttributes.Local
                                .Where(x => x.WS_OIDN == roadSegmentRemoved.Id)
                                .Concat(await context.RoadSegmentEuropeanRoadAttributes
                                    .Where(x => x.WS_OIDN == roadSegmentRemoved.Id)
                                    .ToArrayAsync(token));

                        context.RoadSegmentEuropeanRoadAttributes.RemoveRange(roadSegmentEuropeanRoadAttributeRecords);
                        break;
                }
            }
        });
    }
}
