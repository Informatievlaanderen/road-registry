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
                    Grensknoop = fixture.Create<bool>(),
                    Provenance = fixture.Create<ProvenanceData>()
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWasAdded(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWasAdded>(composer =>
            composer.FromFactory(_ =>
                {
                    var geometry = fixture.Create<RoadSegmentGeometry>();

                    return new RoadSegmentWasAdded
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalId = fixture.Create<RoadSegmentId>(),
                        Geometry = geometry,
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(fixture.Create<RoadSegmentAccessRestrictionV2>(), geometry),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(fixture.Create<RoadSegmentCategoryV2>(), geometry),
                        Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(fixture.Create<RoadSegmentMorphologyV2>(), geometry),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(fixture.Create<RoadSegmentStatusV2>(), geometry),
                        StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(fixture.Create<StreetNameLocalId>(), geometry),
                        MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(fixture.Create<OrganizationId>(), geometry),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(fixture.Create<RoadSegmentSurfaceTypeV2>(), geometry),
                        CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(fixture.Create<bool>(), geometry),
                        EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                        NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()],
                        Provenance = fixture.Create<ProvenanceData>()
                    };
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
                    var geometry = fixture.Create<RoadSegmentGeometry>();

                    return new RoadSegmentWasModified
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalId = roadSegmentId,
                        Geometry = geometry,
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(fixture.Create<RoadSegmentAccessRestrictionV2>(), geometry),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(fixture.Create<RoadSegmentCategoryV2>(), geometry),
                        Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(fixture.Create<RoadSegmentMorphologyV2>(), geometry),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(fixture.Create<RoadSegmentStatusV2>(), geometry),
                        StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(fixture.Create<StreetNameLocalId>(), geometry),
                        MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(fixture.Create<OrganizationId>(), geometry),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(fixture.Create<RoadSegmentSurfaceTypeV2>(), geometry),
                        CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(fixture.Create<bool>(), geometry),
                        Provenance = fixture.Create<ProvenanceData>()
                    };
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWasMerged(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWasMerged>(composer =>
            composer.FromFactory(_ =>
                {
                    var roadSegmentIds = fixture.CreateMany<RoadSegmentId>().ToArray();
                    var geometry = fixture.Create<RoadSegmentGeometry>();

                    return new RoadSegmentWasMerged
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalIds = roadSegmentIds,
                        Geometry = geometry,
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(fixture.Create<RoadSegmentAccessRestrictionV2>(), geometry),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(fixture.Create<RoadSegmentCategoryV2>(), geometry),
                        Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(fixture.Create<RoadSegmentMorphologyV2>(), geometry),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(fixture.Create<RoadSegmentStatusV2>(), geometry),
                        StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(fixture.Create<StreetNameLocalId>(), geometry),
                        MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(fixture.Create<OrganizationId>(), geometry),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(fixture.Create<RoadSegmentSurfaceTypeV2>(), geometry),
                        CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(fixture.Create<bool>(), geometry),
                        EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                        NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()],
                        Provenance = fixture.Create<ProvenanceData>()
                    };
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWasMigrated(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWasMigrated>(composer =>
            composer.FromFactory(_ =>
                {
                    var roadSegmentId = fixture.Create<RoadSegmentId>();
                    var geometry = fixture.Create<RoadSegmentGeometry>();

                    return new RoadSegmentWasMigrated
                    {
                        RoadSegmentId = fixture.Create<RoadSegmentId>(),
                        OriginalId = roadSegmentId,
                        Geometry = geometry,
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
                        AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(fixture.Create<RoadSegmentAccessRestrictionV2>(), geometry),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(fixture.Create<RoadSegmentCategoryV2>(), geometry),
                        Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(fixture.Create<RoadSegmentMorphologyV2>(), geometry),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(fixture.Create<RoadSegmentStatusV2>(), geometry),
                        StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(fixture.Create<StreetNameLocalId>(), geometry),
                        MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(fixture.Create<OrganizationId>(), geometry),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(fixture.Create<RoadSegmentSurfaceTypeV2>(), geometry),
                        CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(fixture.Create<VehicleAccess>(), geometry),
                        PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(fixture.Create<bool>(), geometry),
                        EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()],
                        NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()],
                        Provenance = fixture.Create<ProvenanceData>()
                    };
                }
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
