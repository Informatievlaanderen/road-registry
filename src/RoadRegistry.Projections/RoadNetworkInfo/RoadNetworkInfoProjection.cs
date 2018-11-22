namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using GeoAPI.Geometries;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using NetTopologySuite.Geometries;

    public class RoadNetworkInfoProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNetworkInfoProjection(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            When<Envelope<BeganRoadNetworkImport>>((context, envelope, token) =>
                context.AddAsync(new RoadNetworkInfo(), token)
            );
            When<Envelope<CompletedRoadNetworkImport>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.CompletedImport = true;
            });
            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.RoadNodeCount += 1;
                info.TotalRoadNodeShapeLength +=
                    new PointShapeContent(
                        GeometryTranslator.TranslateM(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
            });
            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.RoadSegmentCount += 1;
                info.TotalRoadSegmentShapeLength +=
                    new PolyLineMShapeContent(
                        GeometryTranslator.Translate(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
                info.RoadSegmentSurfaceAttributeCount += envelope.Message.Surfaces.Length;
                info.RoadSegmentLaneAttributeCount += envelope.Message.Lanes.Length;
                info.RoadSegmentWidthAttributeCount += envelope.Message.Widths.Length;
                info.RoadSegmentEuropeanRoadAttributeCount += envelope.Message.PartOfEuropeanRoads.Length;
                info.RoadSegmentNationalRoadAttributeCount += envelope.Message.PartOfNationalRoads.Length;
                info.RoadSegmentNumberedRoadAttributeCount += envelope.Message.PartOfNumberedRoads.Length;
            });
            When<Envelope<ImportedReferencePoint>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.ReferencePointCount += 1;
                info.TotalReferencePointShapeLength +=
                    new PointShapeContent(
                        reader.ReadAs<PointM>(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
            });
            When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.GradeSeparatedJunctionCount += 1;
            });
            When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
            {
                var info = context.RoadNetworkInfo.Local.SingleOrDefault() ??
                           await context.RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
                info.OrganizationCount += 1;
            });
        }
    }
}
