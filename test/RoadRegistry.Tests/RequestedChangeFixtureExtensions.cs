namespace RoadRegistry.Tests;

using AutoFixture;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;

public static class RequestedChangeFixtureExtensions
{
    public static void CustomizeAddGradeSeparatedJunction(this IFixture fixture)
    {
        fixture.Customize<AddGradeSeparatedJunction>(
            composer =>
                composer.FromFactory(random =>
                    new AddGradeSeparatedJunction
                    {
                        TemporaryId = fixture.Create<GradeSeparatedJunctionId>(),
                        UpperSegmentId = fixture.Create<RoadSegmentId>(),
                        LowerSegmentId = fixture.Create<RoadSegmentId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeAddRoadNode(this IFixture fixture)
    {
        fixture.Customize<AddRoadNode>(composer =>
            composer.FromFactory(random =>
                new AddRoadNode
                {
                    TemporaryId = fixture.Create<RoadNodeId>(),
                    Type = fixture.Create<RoadNodeType>(),
                    Geometry = fixture.Create<RoadNodeGeometry>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeAddRoadSegment(this IFixture fixture)
    {
        fixture.Customize<AddRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegment
                    {
                        TemporaryId = fixture.Create<RoadSegmentId>(),
                        PermanentId = fixture.Create<RoadSegmentId>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        MaintenanceAuthority = fixture.Create<string>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        LeftSideStreetNameId = fixture.Create<int?>(),
                        RightSideStreetNameId = fixture.Create<int?>(),
                        Lanes = fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                        Widths = fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                        Surfaces = fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                    }).OmitAutoProperties()
        );
    }

    public static void CustomizeAddRoadSegmentToEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<AddRoadSegmentToEuropeanRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToEuropeanRoad
                    {
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeAddRoadSegmentToNationalRoad(this IFixture fixture)
    {
        fixture.Customize<AddRoadSegmentToNationalRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToNationalRoad
                    {
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeAddRoadSegmentToNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<AddRoadSegmentToNumberedRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToNumberedRoad
                    {
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeModifyGradeSeparatedJunction(this IFixture fixture)
    {
        fixture.Customize<ModifyGradeSeparatedJunction>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyGradeSeparatedJunction
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperSegmentId = fixture.Create<RoadSegmentId>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeModifyRoadNode(this IFixture fixture)
    {
        fixture.Customize<ModifyRoadNode>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyRoadNode
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = fixture.Create<RoadNodeGeometry>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeModifyRoadSegment(this IFixture fixture)
    {
        fixture.Customize<ModifyRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyRoadSegment
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        MaintenanceAuthority = fixture.Create<OrganizationId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        LeftSideStreetNameId = fixture.Create<int?>(),
                        RightSideStreetNameId = fixture.Create<int?>(),
                        Lanes = fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                        Widths = fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                        Surfaces = fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeModifyRoadSegmentAttributes(this IFixture fixture)
    {
        fixture.Customize<ModifyRoadSegmentAttributes>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyRoadSegmentAttributes
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),

                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        MaintenanceAuthority = fixture.Create<OrganizationId>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        Lanes = fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                        Widths = fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                        Surfaces = fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeModifyRoadSegmentGeometry(this IFixture fixture)
    {
        fixture.Customize<ModifyRoadSegmentGeometry>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyRoadSegmentGeometry
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),

                        Geometry = fixture.Create<RoadSegmentGeometry>(),
                        Lanes = fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                        Widths = fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                        Surfaces = fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveGradeSeparatedJunction(this IFixture fixture)
    {
        fixture.Customize<RemoveGradeSeparatedJunction>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveGradeSeparatedJunction
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadNode(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadNode>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadNode
                    {
                        Id = fixture.Create<RoadNodeId>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadSegment(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadSegment
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadSegments(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadSegments>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadSegments
                    {
                        Ids = fixture.CreateMany<RoadSegmentId>(1).Select(x => x.ToInt32()).ToArray(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveOutlinedRoadSegment(this IFixture fixture)
    {
        fixture.Customize<RemoveOutlinedRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveOutlinedRoadSegment
                    {
                        Id = fixture.Create<RoadSegmentId>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveOutlinedRoadSegmentFromRoadNetwork(this IFixture fixture)
    {
        fixture.Customize<RemoveOutlinedRoadSegmentFromRoadNetwork>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveOutlinedRoadSegmentFromRoadNetwork
                    {
                        Id = fixture.Create<RoadSegmentId>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadSegmentFromEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadSegmentFromEuropeanRoad>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadSegmentFromEuropeanRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadSegmentFromNationalRoad(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadSegmentFromNationalRoad>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadSegmentFromNationalRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
    }

    public static void CustomizeRemoveRoadSegmentFromNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<RemoveRoadSegmentFromNumberedRoad>(
            composer =>
                composer.FromFactory(random =>
                    new RemoveRoadSegmentFromNumberedRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentGeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
    }
}
