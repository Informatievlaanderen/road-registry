namespace RoadRegistry.Tests.AggregateTests.Framework;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using Point = NetTopologySuite.Geometries.Point;

public static class FixtureExtensions
{
    public static void CustomizeRoadNodeWasAdded(this IFixture fixture)
    {
        fixture.Customize<RoadNodeWasAdded>(composer =>
            composer.FromFactory(_ =>
                new RoadNodeWasAdded
                {
                    RoadNodeId = fixture.Create<RoadNodeId>(),
                    OriginalId = fixture.Create<RoadNodeId>(),
                    Geometry = fixture.Create<RoadNodeGeometry>(),
                    Type = fixture.Create<RoadNodeTypeV2>(),
                    Grensknoop = fixture.Create<bool>(),
                    Provenance = fixture.Create<ProvenanceData>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeWasModified(this IFixture fixture)
    {
        fixture.Customize<RoadNodeWasModified>(composer =>
            composer.FromFactory(_ =>
                new RoadNodeWasModified
                {
                    RoadNodeId = fixture.Create<RoadNodeId>(),
                    Geometry = fixture.Create<RoadNodeGeometry>(),
                    Type = fixture.Create<RoadNodeTypeV2>(),
                    Provenance = fixture.Create<ProvenanceData>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWasAdded(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWasAdded>(composer =>
            composer.FromFactory(_ =>
                new RoadSegmentWasAdded
                {
                    RoadSegmentId = fixture.Create<RoadSegmentId>(),
                    OriginalId = fixture.Create<RoadSegmentId>(),
                    Geometry = fixture.Create<RoadSegmentGeometry>(),
                    StartNodeId = fixture.Create<RoadNodeId>(),
                    EndNodeId = fixture.Create<RoadNodeId>(),
                    GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                    AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>>(),
                    Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>>(),
                    Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>>(),
                    Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>>(),
                    StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                    MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                    SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>>(),
                    CarAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                    BikeAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                    PedestrianAccess = fixture.Create<RoadSegmentDynamicAttributeValues<bool>>(),
                    EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                    NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()],
                    Provenance = fixture.Create<ProvenanceData>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWasModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWasModified>(composer =>
            composer.FromFactory(_ =>
                {
                    var roadSegmentId = fixture.Create<RoadSegmentId>();

                    return new RoadSegmentWasModified
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalId = roadSegmentId,
                        Geometry = fixture.Create<RoadSegmentGeometry>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>>(),
                        Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>>(),
                        Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>>(),
                        Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>>(),
                        StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                        MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                        SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>>(),
                        CarAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                        BikeAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                        PedestrianAccess = fixture.Create<RoadSegmentDynamicAttributeValues<bool>>(),
                        Provenance = fixture.Create<ProvenanceData>()
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
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>>(),
                        Category = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>>(),
                        Morphology = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>>(),
                        Status = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>>(),
                        StreetNameId = fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
                        MaintenanceAuthorityId = fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
                        SurfaceType = fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>>(),
                        CarAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                        BikeAccess = fixture.Create<RoadSegmentDynamicAttributeValues<VehicleAccess>>(),
                        PedestrianAccess = fixture.Create<RoadSegmentDynamicAttributeValues<bool>>(),
                        EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                        NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()]
                    }
                ).OmitAutoProperties()
        );
    }
}
