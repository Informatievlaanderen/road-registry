namespace RoadRegistry.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Aiv.Vbr.Shaperon;
    using Events;
    using GeoAPI.Geometries;
    using Microsoft.EntityFrameworkCore;
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
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.CompletedImport = true;
            });
            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) => 
            {
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.RoadNodeCount += 1;
                info.TotalRoadNodeShapeLength += 
                    new PointShapeContent(
                        reader.ReadAs<PointM>(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
            });
            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) => 
            {
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.RoadSegmentCount += 1;
                info.TotalRoadSegmentShapeLength += 
                    new PolyLineMShapeContent(
                        reader.TryReadAs(envelope.Message.Geometry, out LineString line)
                        ? new MultiLineString(new ILineString[] { line })
                        : reader.ReadAs<MultiLineString>(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
                info.RoadSegmentDynamicHardeningAttributeCount += envelope.Message.Hardenings.Length;
                info.RoadSegmentDynamicLaneAttributeCount += envelope.Message.Lanes.Length;
                info.RoadSegmentDynamicWidthAttributeCount += envelope.Message.Widths.Length;
                info.RoadSegmentEuropeanRoadAttributeCount += envelope.Message.PartOfEuropeanRoads.Length;
                info.RoadSegmentNationalRoadAttributeCount += envelope.Message.PartOfNationalRoads.Length;
                info.RoadSegmentNumberedRoadAttributeCount += envelope.Message.PartOfNumberedRoads.Length;
            });
            When<Envelope<ImportedReferencePoint>>(async (context, envelope, token) => 
            {
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.ReferencePointCount += 1;
                info.TotalReferencePointShapeLength =
                    new PointShapeContent(
                        reader.ReadAs<PointM>(envelope.Message.Geometry)
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
            });
            When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) => 
            {
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.GradeSeparatedJunctionCount += 1;
            });
            When<Envelope<ImportedOrganization>>(async (context, envelope, token) => 
            {
                var info = await context.RoadNetworkInfo.SingleAsync(token);
                info.OrganizationCount += 1;
            });
        }
    }
}
