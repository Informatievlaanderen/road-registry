namespace RoadRegistry.Tests;

using AutoFixture;
using BackOffice;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using Point = NetTopologySuite.Geometries.Point;

public static class Customizations
{
    public static T CreateWhichIsDifferentThan<T>(this IFixture fixture, T illegalValue)
    {
        var value = fixture.Create<T>();

        while (Equals(value, illegalValue))
        {
            value = fixture.Create<T>();
        }

        return value;
    }

    public static void CustomizeGradeSeparatedJunctionAdded(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionAdded
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        TemporaryId = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeGradeSeparatedJunctionModified(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionModified>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionModified
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeGradeSeparatedJunctionRemoved(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionRemoved
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedGradeSeparatedJunction(this IFixture fixture)
    {
        fixture.Customize<ImportedGradeSeparatedJunction>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedGradeSeparatedJunction
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadNode(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadNode>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadNode
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>()),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegment(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegment>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegment
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Version = fixture.Create<int>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        LeftSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                        RightSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                        Lanes = fixture.CreateMany<ImportedRoadSegmentLaneAttribute>(generator.Next(0, 10)).ToArray(),
                        Widths = fixture.CreateMany<ImportedRoadSegmentWidthAttribute>(generator.Next(0, 10)).ToArray(),
                        Surfaces = fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfEuropeanRoads = fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfNationalRoads = fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfNumberedRoads = fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        RecordingDate = fixture.Create<DateTime>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentEuropeanRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentEuropeanRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentEuropeanRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<EuropeanRoadNumber>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentLaneAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentLaneAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentLaneAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Count = fixture.Create<RoadSegmentLaneCount>(),
                        Direction = fixture.Create<RoadSegmentLaneDirection>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentNationalRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentNationalRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentNationalRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<NationalRoadNumber>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentNumberedRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentNumberedRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentNumberedRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentSideAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentSideAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentSideAttribute
                    {
                        StreetNameId = fixture.Create<int?>(),
                        StreetName = fixture.Create<string>(),
                        MunicipalityNISCode = fixture.Create<string>(),
                        Municipality = fixture.Create<string>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentSurfaceAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentSurfaceAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Type = fixture.Create<RoadSegmentSurfaceType>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentWidthAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentWidthAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentWidthAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Width = fixture.Create<RoadSegmentWidth>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNetworkChangesAccepted(this IFixture fixture)
    {
        fixture.Customize<RoadNetworkChangesAccepted>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNetworkChangesAccepted
                    {
                        RequestId = fixture.Create<ArchiveId>(),
                        Reason = fixture.Create<Reason>(),
                        Operator = fixture.Create<OperatorName>(),
                        OrganizationId = fixture.Create<OrganizationId>(),
                        Organization = fixture.Create<OrganizationName>(),
                        TransactionId = fixture.Create<TransactionId>(),
                        Changes = fixture.CreateMany<AcceptedChange>(generator.Next(1, 5)).ToArray(),
                        When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeAdded(this IFixture fixture)
    {
        fixture.Customize<RoadNodeAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeAdded
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        TemporaryId = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>())
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeModified(this IFixture fixture)
    {
        fixture.Customize<RoadNodeModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeModified
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>())
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeRemoved(this IFixture fixture)
    {
        fixture.Customize<RoadNodeRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeRemoved
                    {
                        Id = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAdded(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAdded
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        TemporaryId = fixture.Create<RoadSegmentId>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(generator.Next(1, 5)).ToArray(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(generator.Next(1, 5)).ToArray(),
                        Version = fixture.Create<int>(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(generator.Next(1, 5)).ToArray(),
                        LeftSide = fixture.Create<RoadSegmentSideAttributes>(),
                        RightSide = fixture.Create<RoadSegmentSideAttributes>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToEuropeanRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToEuropeanRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToNationalRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToNationalRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToNumberedRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentLaneAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentLaneAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentLaneAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Count = fixture.Create<RoadSegmentLaneCount>(),
                        Direction = fixture.Create<RoadSegmentLaneDirection>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentModified
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(generator.Next(1, 5)).ToArray(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(generator.Next(1, 5)).ToArray(),
                        Version = fixture.Create<int>(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(generator.Next(1, 5)).ToArray(),
                        LeftSide = fixture.Create<RoadSegmentSideAttributes>(),
                        RightSide = fixture.Create<RoadSegmentSideAttributes>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentOnNumberedRoadModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentOnNumberedRoadModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentOnNumberedRoadModified
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemoved(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemoved
                    {
                        Id = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromEuropeanRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromEuropeanRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromNationalRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromNationalRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromNationalRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromNumberedRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromNumberedRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentSurfaceAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentSurfaceAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentSurfaceAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Type = fixture.Create<RoadSegmentSurfaceType>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWidthAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWidthAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentWidthAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Width = fixture.Create<RoadSegmentWidth>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }
}
