// namespace RoadRegistry.Wfs.Projections
// {
//     using System;
//     using System.Linq;
//     using System.Threading;
//     using System.Threading.Tasks;
//     using BackOffice;
//     using BackOffice.Messages;
//     using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
//     using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
//     using Microsoft.EntityFrameworkCore;
//     using NetTopologySuite.Geometries;
//     using Schema;
//
//     public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<WfsContext>
//     {
//         public GradeSeparatedJunctionRecordProjection()
//         {
//             When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
//             {
//                 var intersections = await ListIntersectionsByRoadSegmentIdsAsync(context, envelope.Message.LowerRoadSegmentId,envelope.Message.UpperRoadSegmentId, token);
//                 await context.GradeSeparatedJunctions.AddAsync(new GradeSeparatedJunctionRecord
//                 {
//                     Id = envelope.Message.Id,
//                     BeginTime = envelope.Message.Origin.Since,
//                     Type = GetGradeSeparatedJunctionTypeDutchTranslation(envelope.Message.Type),
//                     LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
//                     UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
//                     IntersectGeometry = intersections.FirstOrDefault()
//                 }, token);
//             });
//
//             When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
//             {
//
//                 foreach (var change in envelope.Message.Changes.Flatten())
//                 {
//                     switch (change)
//                     {
//                         case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
//                             await AddGradeSeparatedJunction(context, envelope, gradeSeparatedJunctionAdded, token);
//                             break;
//
//                         case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
//                             await ModifyGradeSeparatedJunction(context, envelope, gradeSeparatedJunctionModified, token);
//                             break;
//
//                         case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
//                             await RemoveGradeSeparatedJunction(gradeSeparatedJunctionRemoved, context);
//                             break;
//                     }
//                 }
//             });
//         }
//
//         private static async Task AddGradeSeparatedJunction(WfsContext context,
//             Envelope<RoadNetworkChangesAccepted> envelope,
//             GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded,
//             CancellationToken token)
//         {
//             var intersections = await ListIntersectionsByRoadSegmentIdsAsync(context, gradeSeparatedJunctionAdded.LowerRoadSegmentId,gradeSeparatedJunctionAdded.UpperRoadSegmentId, token);
//             await context.GradeSeparatedJunctions.AddAsync(new GradeSeparatedJunctionRecord
//             {
//                 Id = gradeSeparatedJunctionAdded.Id,
//                 BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
//                 Type = GetGradeSeparatedJunctionTypeDutchTranslation(gradeSeparatedJunctionAdded.Type),
//                 LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
//                 UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
//                 IntersectGeometry = intersections.FirstOrDefault()
//             }, token);
//         }
//
//         private static async Task ModifyGradeSeparatedJunction(WfsContext context,
//             Envelope<RoadNetworkChangesAccepted> envelope,
//             GradeSeparatedJunctionModified gradeSeparatedJunctionModified,
//             CancellationToken token)
//         {
//             var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionModified.Id, token).ConfigureAwait(false);
//
//             if (gradeSeparatedJunctionRecord == null)
//             {
//                 throw new InvalidOperationException($"GradeSeparatedJunctionRecord with id {gradeSeparatedJunctionModified.Id} is not found");
//             }
//             var intersections = await ListIntersectionsByRoadSegmentIdsAsync(context, gradeSeparatedJunctionModified.LowerRoadSegmentId,gradeSeparatedJunctionModified.UpperRoadSegmentId, token);
//             gradeSeparatedJunctionRecord.Id = gradeSeparatedJunctionModified.Id;
//             gradeSeparatedJunctionRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
//             gradeSeparatedJunctionRecord.Type = GetGradeSeparatedJunctionTypeDutchTranslation(gradeSeparatedJunctionModified.Type);
//             gradeSeparatedJunctionRecord.LowerRoadSegmentId = gradeSeparatedJunctionModified.LowerRoadSegmentId;
//             gradeSeparatedJunctionRecord.UpperRoadSegmentId = gradeSeparatedJunctionModified.UpperRoadSegmentId;
//             gradeSeparatedJunctionRecord.IntersectGeometry = intersections.FirstOrDefault();
//         }
//
//         private static async Task RemoveGradeSeparatedJunction(GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved, WfsContext context)
//         {
//             var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionRemoved.Id).ConfigureAwait(false);
//
//             if (gradeSeparatedJunctionRecord == null)
//             {
//                 return;
//             }
//
//             context.GradeSeparatedJunctions.Remove(gradeSeparatedJunctionRecord);
//         }
//
//         private static async Task<Geometry[]> ListIntersectionsByRoadSegmentIdsAsync(WfsContext context,
//             int lowerRoadSegmentId, int upperRoadSegmentId, CancellationToken cancellationToken)
//         {
//
//             var lowerRoadSegment = await context.RoadSegments.FirstOrDefaultAsync(i => i.Id == lowerRoadSegmentId, cancellationToken);
//             var upperRoadSegment = await context.RoadSegments.FirstOrDefaultAsync(i => i.Id == upperRoadSegmentId, cancellationToken);
//
//             if (lowerRoadSegment?.Geometry2D == null || upperRoadSegment?.Geometry2D == null)
//             {
//                 return null;
//             }
//
//             var result = lowerRoadSegment.Geometry2D.Intersection(upperRoadSegment.Geometry2D);
//             if (result.OgcGeometryType == OgcGeometryType.GeometryCollection)
//             {
//                 return ((GeometryCollection)result).Geometries;
//             }
//             return new [] { lowerRoadSegment.Geometry2D.Intersection(upperRoadSegment.Geometry2D) };
//         }
//
//         private static string GetGradeSeparatedJunctionTypeDutchTranslation(string gradeSeparatedJunctionType) =>
//             GradeSeparatedJunctionType.CanParse(gradeSeparatedJunctionType)
//                 ? GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionType).Translation.Name
//                 : gradeSeparatedJunctionType;
//     }
// }

