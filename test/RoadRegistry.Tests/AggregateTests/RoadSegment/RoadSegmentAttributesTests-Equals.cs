namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using System.Collections.Immutable;
using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributesEqualsTests
{
    [Fact]
    public void WithIdentical_ThenTrue()
    {
        var attributes1 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestriction.PublicRoad),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategory.EuropeanMainRoad),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphology.Motorway),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>()
                .Add(RoadSegmentStatus.InUse),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(RoadSegmentSurfaceType.SolidSurface),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        var attributes2 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestriction.PublicRoad),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategory.EuropeanMainRoad),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphology.Motorway),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>()
                .Add(RoadSegmentStatus.InUse),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(RoadSegmentSurfaceType.SolidSurface),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        attributes1.Equals(attributes2).Should().BeTrue();
    }

    [Fact]
    public void WithDifference_ThenFalse()
    {
        var attributes1 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>().Add(RoadSegmentAccessRestriction.PublicRoad),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>().Add(RoadSegmentCategory.EuropeanMainRoad),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>().Add(RoadSegmentMorphology.Motorway),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>().Add(RoadSegmentStatus.InUse),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(RoadSegmentSurfaceType.SolidSurface),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        var attributes2 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>().Add(RoadSegmentAccessRestriction.PublicRoad),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>().Add(RoadSegmentCategory.EuropeanMainRoad),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>().Add(RoadSegmentMorphology.Motorway),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>().Add(RoadSegmentStatus.InUse),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(2)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(RoadSegmentSurfaceType.SolidSurface),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        attributes1.Equals(attributes2).Should().BeFalse();
    }
}
