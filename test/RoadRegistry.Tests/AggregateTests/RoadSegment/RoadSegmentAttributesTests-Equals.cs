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
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.OpenbareWeg),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategoryV2.EuropeseHoofdweg),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphologyV2.Autosnelweg),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>()
                .Add(RoadSegmentStatusV2.Gepland),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>().Add(RoadSegmentSurfaceTypeV2.Verhard),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        var attributes2 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.OpenbareWeg),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategoryV2.EuropeseHoofdweg),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphologyV2.Autosnelweg),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>()
                .Add(RoadSegmentStatusV2.Gepland),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>().Add(RoadSegmentSurfaceTypeV2.Verhard),
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
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.OpenbareWeg),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategoryV2.EuropeseHoofdweg),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphologyV2.Autosnelweg),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>()
                .Add(RoadSegmentStatusV2.Gepland),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(1)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>().Add(RoadSegmentSurfaceTypeV2.Verhard),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        var attributes2 = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.OpenbareWeg),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Left, RoadSegmentCategoryV2.EuropeseHoofdweg),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new RoadSegmentPosition(0), new RoadSegmentPosition(1), RoadSegmentAttributeSide.Right, RoadSegmentMorphologyV2.Autosnelweg),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>()
                .Add(RoadSegmentStatusV2.Gepland),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>().Add(new StreetNameLocalId(2)),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>().Add(OrganizationId.DigitaalVlaanderen),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>().Add(RoadSegmentSurfaceTypeV2.Verhard),
            EuropeanRoadNumbers = ImmutableList.Create(EuropeanRoadNumber.E40),
            NationalRoadNumbers = ImmutableList.Create(NationalRoadNumber.Parse("N1"))
        };

        attributes1.Equals(attributes2).Should().BeFalse();
    }
}
