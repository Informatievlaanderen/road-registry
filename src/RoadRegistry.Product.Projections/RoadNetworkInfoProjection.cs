namespace RoadRegistry.Product.Projections
{
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Schema;

    public class RoadNetworkInfoProjection : ConnectedProjection<ProductContext>
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
                        case RoadNodeAdded m:
                            info.RoadNodeCount++;
                            info.TotalRoadNodeShapeLength +=
                                new PointShapeContent(
                                        GeometryTranslator.FromGeometryPoint(BackOffice.Core.GeometryTranslator.Translate(m.Geometry))
                                    )
                                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                                    .ToInt32();
                            break;

                        case RoadSegmentAdded m:
                            info.RoadSegmentCount += 1;
                            info.TotalRoadSegmentShapeLength +=
                                new PolyLineMShapeContent(
                                        GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(m.Geometry))
                                    )
                                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                                    .ToInt32();
                            //Note that in order to support deletion and modification we'll need to track it per segment
                            info.RoadSegmentSurfaceAttributeCount += m.Surfaces.Length;
                            info.RoadSegmentLaneAttributeCount += m.Lanes.Length;
                            info.RoadSegmentWidthAttributeCount += m.Widths.Length;
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


                info.OrganizationCount += 1;
            });
        }
    }
}
