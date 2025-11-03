// namespace RoadRegistry.Wms.Projections;
//
// using System.Linq;
// using System.Threading.Tasks;
// using BackOffice.Messages;
// using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
// using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
// using Microsoft.EntityFrameworkCore;
// using Schema;
//
// public class RoadSegmentNationalRoadAttributeRecordProjection : ConnectedProjection<WmsContext>
// {
//     public RoadSegmentNationalRoadAttributeRecordProjection()
//     {
//         When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
//         {
//             if (envelope.Message.PartOfNationalRoads.Length == 0)
//             {
//                 return Task.CompletedTask;
//             }
//
//             var nationalRoadAttributes = envelope.Message
//                 .PartOfNationalRoads
//                 .Select(nationalRoad => new RoadSegmentNationalRoadAttributeRecord
//                 {
//                     NW_OIDN = nationalRoad.AttributeId,
//                     WS_OIDN = envelope.Message.Id,
//                     IDENT2 = nationalRoad.Number,
//                     BEGINTIJD = nationalRoad.Origin.Since,
//                     BEGINORG = nationalRoad.Origin.OrganizationId,
//                     LBLBGNORG = nationalRoad.Origin.Organization
//                 });
//
//             return context.RoadSegmentNationalRoadAttributes.AddRangeAsync(nationalRoadAttributes, token);
//         });
//
//         When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
//         {
//             foreach (var change in envelope.Message.Changes.Flatten())
//             {
//                 switch (change)
//                 {
//                     case RoadSegmentAddedToNationalRoad nationalRoad:
//                         await context.RoadSegmentNationalRoadAttributes.AddAsync(new RoadSegmentNationalRoadAttributeRecord
//                         {
//                             NW_OIDN = nationalRoad.AttributeId,
//                             WS_OIDN = nationalRoad.SegmentId,
//                             IDENT2 = nationalRoad.Number,
//                             BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
//                             BEGINORG = envelope.Message.OrganizationId,
//                             LBLBGNORG = envelope.Message.Organization
//                         });
//                         break;
//                     case RoadSegmentRemovedFromNationalRoad nationalRoad:
//                         var roadSegmentNationalRoadAttributeRecord =
//                             await context.RoadSegmentNationalRoadAttributes.FindAsync(nationalRoad.AttributeId, cancellationToken: token).ConfigureAwait(false);
//
//                         if (roadSegmentNationalRoadAttributeRecord is not null)
//                         {
//                             context.RoadSegmentNationalRoadAttributes.Remove(roadSegmentNationalRoadAttributeRecord);
//                         }
//
//                         break;
//                     case RoadSegmentRemoved roadSegmentRemoved:
//                         var roadSegmentNationalRoadAttributeRecords =
//                             context.RoadSegmentNationalRoadAttributes.Local
//                                 .Where(x => x.WS_OIDN == roadSegmentRemoved.Id)
//                                 .Concat(await context.RoadSegmentNationalRoadAttributes
//                                     .Where(x => x.WS_OIDN == roadSegmentRemoved.Id)
//                                     .ToArrayAsync(token));
//
//                         context.RoadSegmentNationalRoadAttributes.RemoveRange(roadSegmentNationalRoadAttributeRecords);
//                         break;
//                 }
//             }
//         });
//     }
// }
