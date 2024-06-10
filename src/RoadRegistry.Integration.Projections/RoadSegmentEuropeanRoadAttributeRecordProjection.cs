// namespace RoadRegistry.Integration.Projections;
//
// using System;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using BackOffice.Extracts.Dbase.RoadSegments;
// using BackOffice.Messages;
// using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
// using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IO;
// using Schema;
// using Schema.Extensions;
//
// public class RoadSegmentEuropeanRoadAttributeRecordProjection : ConnectedProjection<IntegrationContext>
// {
//     public RoadSegmentEuropeanRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
//         Encoding encoding)
//     {
//         ArgumentNullException.ThrowIfNull(manager);
//         ArgumentNullException.ThrowIfNull(encoding);
//
//         When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
//         {
//             if (envelope.Message.PartOfEuropeanRoads.Length == 0)
//                 return Task.CompletedTask;
//
//             var europeanRoadAttributes = envelope.Message
//                 .PartOfEuropeanRoads
//                 .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
//                 {
//                     Id = europeanRoad.AttributeId,
//                     RoadSegmentId = envelope.Message.Id,
//                     DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
//                     {
//                         EU_OIDN = { Value = europeanRoad.AttributeId },
//                         WS_OIDN = { Value = envelope.Message.Id },
//                         EUNUMMER = { Value = europeanRoad.Number },
//                         BEGINTIJD = { Value = europeanRoad.Origin.Since },
//                         BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
//                         LBLBGNORG = { Value = europeanRoad.Origin.Organization }
//                     }.ToBytes(manager, encoding)
//                 });
//
//             return context.AddRangeAsync(europeanRoadAttributes, token);
//         });
//
//         When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
//         {
//             foreach (var change in envelope.Message.Changes.Flatten())
//                 switch (change)
//                 {
//                     case RoadSegmentAddedToEuropeanRoad europeanRoad:
//                         await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeRecord
//                         {
//                             Id = europeanRoad.AttributeId,
//                             RoadSegmentId = europeanRoad.SegmentId,
//                             DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
//                             {
//                                 EU_OIDN = { Value = europeanRoad.AttributeId },
//                                 WS_OIDN = { Value = europeanRoad.SegmentId },
//                                 EUNUMMER = { Value = europeanRoad.Number },
//                                 BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
//                                 BEGINORG = { Value = envelope.Message.OrganizationId },
//                                 LBLBGNORG = { Value = envelope.Message.Organization }
//                             }.ToBytes(manager, encoding)
//                         });
//                         break;
//                     case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
//                         var roadSegmentEuropeanRoadAttributeRecord = await context.RoadSegmentEuropeanRoadAttributes.FindAsync(europeanRoad.AttributeId, cancellationToken: token).ConfigureAwait(false);
//                         if (roadSegmentEuropeanRoadAttributeRecord is not null)
//                         {
//                             context.RoadSegmentEuropeanRoadAttributes.Remove(roadSegmentEuropeanRoadAttributeRecord);
//                         }
//                         break;
//
//                     case RoadSegmentRemoved roadSegmentRemoved:
//                         context.RoadSegmentEuropeanRoadAttributes.RemoveRange(
//                             context.RoadSegmentEuropeanRoadAttributes.Local
//                             .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id)
//                             .Concat(await context.RoadSegmentEuropeanRoadAttributes
//                                 .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id)
//                                 .ToArrayAsync(token)));
//                         break;
//                 }
//         });
//     }
// }
