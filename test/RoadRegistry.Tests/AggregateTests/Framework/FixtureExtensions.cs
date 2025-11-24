namespace RoadRegistry.Tests.AggregateTests.Framework;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
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

    public static void CustomizeRoadSegmentAdded(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAdded>(composer =>
            composer.FromFactory(_ =>
                new RoadSegmentAdded
                {
                    RoadSegmentId = fixture.Create<RoadSegmentId>(),
                    OriginalId = fixture.Create<RoadSegmentId>(),
                    Geometry = fixture.Create<MultiLineString>().ToGeometryObject(),
                    StartNodeId = fixture.Create<RoadNodeId>(),
                    EndNodeId = fixture.Create<RoadNodeId>(),
                    GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                    AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
                    Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
                    Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
                    Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
                    StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                    MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                    SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
                    EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                    NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()]
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentModified>(composer =>
            composer.FromFactory(_ =>
                {
                    var roadSegmentId = fixture.Create<RoadSegmentId>();

                    return new RoadSegmentModified
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalId = roadSegmentId,
                        Geometry = fixture.Create<MultiLineString>().ToGeometryObject(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
                        Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
                        Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
                        Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
                        StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                        MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                        SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>()
                    };
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

    public static void CustomizeRoadSegmentAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAttributes>(
            composer =>
                composer.FromFactory(_ =>
                    new RoadSegmentAttributes
                    {
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
                        Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
                        Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
                        Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
                        StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                        MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                        SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
                        EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                        NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()]
                    }
                ).OmitAutoProperties()
        );
    }
}
