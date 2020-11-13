namespace RoadRegistry.Editor.Projections
{
    using System.Linq;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Schema;

    public class RoadNetworkInfoProjection : ConnectedProjection<EditorContext>
    {
        public RoadNetworkInfoProjection()
        {
            When<Envelope<BeganRoadNetworkImport>>(async (context, envelope, token) =>
                await context.RoadNetworkInfo.AddAsync(new RoadNetworkInfo(), token)
            );
            When<Envelope<CompletedRoadNetworkImport>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                info.CompletedImport = true;
            });
            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                info.RoadNodeCount += 1;
                info.TotalRoadNodeShapeLength +=
                    new PointShapeContent(
                        GeometryTranslator.FromGeometryPoint(BackOffice.Core.GeometryTranslator.Translate(envelope.Message.Geometry))
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32();
            });
            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                info.RoadSegmentCount += 1;
                info.TotalRoadSegmentShapeLength +=
                    new PolyLineMShapeContent(
                        GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(envelope.Message.Geometry))
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
            When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                info.GradeSeparatedJunctionCount += 1;
            });
            When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                info.OrganizationCount += 1;
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                var info = await context.GetRoadNetworkInfo(token);
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadNodeAdded _:
                            info.RoadNodeCount++;
                            info.TotalRoadNodeShapeLength +=
                                PointShapeContent.Length.Plus(ShapeRecord.HeaderLength).ToInt32();
                            break;

                        case RoadNodeRemoved _:
                            info.RoadNodeCount--;
                            info.TotalRoadNodeShapeLength -=
                                PointShapeContent.Length.Plus(ShapeRecord.HeaderLength).ToInt32();
                            break;

                        case RoadSegmentAdded m:
                            await OnRoadSegmentAdded(info, m, context);
                            break;

                        case RoadSegmentModified m:
                            await OnRoadSegmentModified(context, m, info);
                            break;

                        case RoadSegmentRemoved m:
                            await OnRoadSegmentRemoved(context, m, info);
                            break;

                        case RoadSegmentAddedToEuropeanRoad _:
                            info.RoadSegmentEuropeanRoadAttributeCount += 1;
                            break;
                        case RoadSegmentAddedToNationalRoad _:
                            info.RoadSegmentNationalRoadAttributeCount += 1;
                            break;
                        case RoadSegmentAddedToNumberedRoad _:
                            info.RoadSegmentNumberedRoadAttributeCount += 1;
                            break;
                        case GradeSeparatedJunctionAdded _:
                            info.GradeSeparatedJunctionCount += 1;
                            break;
                    }
                }
            });
        }

        private static async Task OnRoadSegmentAdded(RoadNetworkInfo info, RoadSegmentAdded m, EditorContext context)
        {
            info.RoadSegmentCount += 1;

            var roadNetworkInfoSegmentCache = new RoadNetworkInfoSegmentCache
            {
                RoadSegmentId = m.Id,
                ShapeLength = new PolyLineMShapeContent(
                        GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(m.Geometry))
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
                SurfacesLength = m.Surfaces.Length,
                LanesLength = m.Lanes.Length,
                WidthsLength = m.Widths.Length
            };

            info.TotalRoadSegmentShapeLength += roadNetworkInfoSegmentCache.ShapeLength;
            info.RoadSegmentSurfaceAttributeCount += roadNetworkInfoSegmentCache.SurfacesLength;
            info.RoadSegmentLaneAttributeCount += roadNetworkInfoSegmentCache.LanesLength;
            info.RoadSegmentWidthAttributeCount += roadNetworkInfoSegmentCache.WidthsLength;

            await context.RoadNetworkInfoSegmentCache.AddAsync(roadNetworkInfoSegmentCache);
        }

        private static async Task OnRoadSegmentModified(EditorContext context, RoadSegmentModified m, RoadNetworkInfo info)
        {
            var oldSegmentCache = await context.RoadNetworkInfoSegmentCache.FindAsync(m.Id);
            var newSegmentCache = new RoadNetworkInfoSegmentCache
            {
                ShapeLength = new PolyLineMShapeContent(
                        GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(m.Geometry))
                    )
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
                SurfacesLength = m.Surfaces.Length,
                LanesLength = m.Lanes.Length,
                WidthsLength = m.Widths.Length
            };

            info.TotalRoadSegmentShapeLength += newSegmentCache.ShapeLength - oldSegmentCache.ShapeLength;
            info.RoadSegmentSurfaceAttributeCount += newSegmentCache.SurfacesLength - oldSegmentCache.SurfacesLength;
            info.RoadSegmentLaneAttributeCount += newSegmentCache.LanesLength - oldSegmentCache.LanesLength;
            info.RoadSegmentWidthAttributeCount += newSegmentCache.WidthsLength - oldSegmentCache.WidthsLength;

            oldSegmentCache.ShapeLength = newSegmentCache.ShapeLength;
            oldSegmentCache.SurfacesLength = newSegmentCache.SurfacesLength;
            oldSegmentCache.LanesLength = newSegmentCache.LanesLength;
            oldSegmentCache.WidthsLength = newSegmentCache.WidthsLength;
        }

        private static async Task OnRoadSegmentRemoved(EditorContext context, RoadSegmentRemoved m, RoadNetworkInfo info)
        {
            var segmentCache = await context.RoadNetworkInfoSegmentCache.FindAsync(m.Id);

            info.TotalRoadSegmentShapeLength -= segmentCache.ShapeLength;
            info.RoadSegmentSurfaceAttributeCount -= segmentCache.SurfacesLength;
            info.RoadSegmentLaneAttributeCount -= segmentCache.LanesLength;
            info.RoadSegmentWidthAttributeCount -= segmentCache.WidthsLength;

            context.RoadNetworkInfoSegmentCache.Remove(segmentCache);
        }
    }
}
