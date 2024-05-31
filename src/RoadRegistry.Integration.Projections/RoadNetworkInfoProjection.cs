namespace RoadRegistry.Integration.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using Schema;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadNetworkInfoProjection : ConnectedProjection<IntegrationContext>
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
                PointShapeContent.Length.Plus(ShapeRecord.HeaderLength).ToInt32();
        });
        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var info = await context.GetRoadNetworkInfo(token);
            info.RoadSegmentCount += 1;

            var roadNetworkInfoSegmentCache = new RoadNetworkInfoSegmentCache
            {
                RoadSegmentId = envelope.Message.Id,
                ShapeLength = new PolyLineMShapeContent(
                        GeometryTranslator.FromGeometryMultiLineString(
                            BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
                SurfacesLength = envelope.Message.Surfaces.Length,
                LanesLength = envelope.Message.Lanes.Length,
                WidthsLength = envelope.Message.Widths.Length,
                PartOfEuropeanRoadsLength = envelope.Message.PartOfEuropeanRoads.Length,
                PartOfNationalRoadsLength = envelope.Message.PartOfNationalRoads.Length,
                PartOfNumberedRoadsLength = envelope.Message.PartOfNumberedRoads.Length
            };

            info.TotalRoadSegmentShapeLength += roadNetworkInfoSegmentCache.ShapeLength;

            info.RoadSegmentSurfaceAttributeCount += roadNetworkInfoSegmentCache.SurfacesLength;
            info.RoadSegmentLaneAttributeCount += roadNetworkInfoSegmentCache.LanesLength;
            info.RoadSegmentWidthAttributeCount += roadNetworkInfoSegmentCache.WidthsLength;

            info.RoadSegmentEuropeanRoadAttributeCount += roadNetworkInfoSegmentCache.PartOfEuropeanRoadsLength;
            info.RoadSegmentNationalRoadAttributeCount += roadNetworkInfoSegmentCache.PartOfNationalRoadsLength;
            info.RoadSegmentNumberedRoadAttributeCount += roadNetworkInfoSegmentCache.PartOfNumberedRoadsLength;

            await context.RoadNetworkInfoSegmentCache.AddAsync(roadNetworkInfoSegmentCache);
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
                        await OnRoadSegmentModified(context, m, info, token);
                        break;

                    case RoadSegmentGeometryModified m:
                        await OnRoadSegmentGeometryModified(context, m, info, token);
                        break;

                    case RoadSegmentRemoved m:
                        await OnRoadSegmentRemoved(context, m, info, token);
                        break;

                    case RoadSegmentAddedToEuropeanRoad _:
                        info.RoadSegmentEuropeanRoadAttributeCount += 1;
                        break;

                    case RoadSegmentRemovedFromEuropeanRoad _:
                        info.RoadSegmentEuropeanRoadAttributeCount -= 1;
                        break;

                    case RoadSegmentAddedToNationalRoad _:
                        info.RoadSegmentNationalRoadAttributeCount += 1;
                        break;

                    case RoadSegmentRemovedFromNationalRoad _:
                        info.RoadSegmentNationalRoadAttributeCount -= 1;
                        break;

                    case RoadSegmentAddedToNumberedRoad _:
                        info.RoadSegmentNumberedRoadAttributeCount += 1;
                        break;

                    case RoadSegmentRemovedFromNumberedRoad _:
                        info.RoadSegmentNumberedRoadAttributeCount -= 1;
                        break;

                    case GradeSeparatedJunctionAdded _:
                        info.GradeSeparatedJunctionCount += 1;
                        break;

                    case GradeSeparatedJunctionRemoved _:
                        info.GradeSeparatedJunctionCount -= 1;
                        break;
                }
        });
    }

    private static async Task OnRoadSegmentAdded(RoadNetworkInfo info, RoadSegmentAdded m, IntegrationContext context)
    {
        info.RoadSegmentCount += 1;

        var roadNetworkInfoSegmentCache = new RoadNetworkInfoSegmentCache
        {
            RoadSegmentId = m.Id,
            ShapeLength = new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(m.Geometry))
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

    private static async Task OnRoadSegmentModified(IntegrationContext context, RoadSegmentModified m, RoadNetworkInfo info, CancellationToken token)
    {
        var oldSegmentCache = await context.RoadNetworkInfoSegmentCache.FindAsync(m.Id, cancellationToken: token).ConfigureAwait(false);
        var newSegmentCache = new RoadNetworkInfoSegmentCache
        {
            ShapeLength = new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(m.Geometry))
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

    private static async Task OnRoadSegmentGeometryModified(IntegrationContext context, RoadSegmentGeometryModified m, RoadNetworkInfo info, CancellationToken token)
    {
        var oldSegmentCache = await context.RoadNetworkInfoSegmentCache.FindAsync(m.Id, cancellationToken: token).ConfigureAwait(false);
        var newSegmentCache = new RoadNetworkInfoSegmentCache
        {
            ShapeLength = new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(m.Geometry))
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

    private static async Task OnRoadSegmentRemoved(IntegrationContext context, RoadSegmentRemoved m, RoadNetworkInfo info, CancellationToken token)
    {
        info.RoadSegmentCount -= 1;

        var segmentCache = await context.RoadNetworkInfoSegmentCache.FindAsync(m.Id, cancellationToken: token).ConfigureAwait(false);

        info.TotalRoadSegmentShapeLength -= segmentCache.ShapeLength;

        info.RoadSegmentSurfaceAttributeCount -= segmentCache.SurfacesLength;
        info.RoadSegmentLaneAttributeCount -= segmentCache.LanesLength;
        info.RoadSegmentWidthAttributeCount -= segmentCache.WidthsLength;

        info.RoadSegmentEuropeanRoadAttributeCount -= segmentCache.PartOfEuropeanRoadsLength;
        info.RoadSegmentNationalRoadAttributeCount -= segmentCache.PartOfNationalRoadsLength;
        info.RoadSegmentNumberedRoadAttributeCount -= segmentCache.PartOfNumberedRoadsLength;

        context.RoadNetworkInfoSegmentCache.Remove(segmentCache);
    }
}
