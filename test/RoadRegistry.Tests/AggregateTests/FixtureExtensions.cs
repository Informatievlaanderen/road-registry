namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using RoadNodeAdded = RoadRegistry.RoadNode.Events.RoadNodeAdded;
using RoadSegmentAdded = RoadRegistry.RoadSegment.Events.RoadSegmentAdded;

public static class FixtureExtensions
{
    public static void CustomizeRoadNodeAdded(this IFixture fixture)
    {
        fixture.Customize<RoadNodeAdded>(composer =>
            composer.FromFactory(_ =>
                new RoadNodeAdded
                {
                    RoadNodeId = fixture.Create<RoadNodeId>(),
                    TemporaryId = fixture.Create<RoadNodeId>(),
                    OriginalId = null,
                    Geometry = fixture.Create<Point>().ToGeometryObject(),
                    Type = fixture.Create<RoadNodeType>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeModified(this IFixture fixture)
    {
        fixture.Customize<RoadNodeModified>(composer =>
            composer.FromFactory(_ =>
                new RoadNodeModified
                {
                    RoadNodeId = fixture.Create<RoadNodeId>(),
                    Geometry = fixture.Create<Point>().ToGeometryObject(),
                    Type = fixture.Create<RoadNodeType>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentDynamicAttributeValues<T>(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentDynamicAttributeValues<T>>(
            composer =>
                composer.FromFactory(_ =>
                    new RoadSegmentDynamicAttributeValues<T>()
                        .Add(fixture.Create<T>())
                ).OmitAutoProperties()
        );
    }
}
