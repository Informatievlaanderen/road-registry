namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using AddRoadSegment = RoadRegistry.BackOffice.Messages.AddRoadSegment;
using RequestedRoadSegmentEuropeanRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentEuropeanRoadAttribute;
using RequestedRoadSegmentNationalRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNationalRoadAttribute;
using RequestedRoadSegmentNumberedRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNumberedRoadAttribute;

public class AddRoadSegmentOutlineValidatorTests : ValidatorTest<AddRoadSegment, AddRoadSegmentValidator>
{
    public AddRoadSegmentOutlineValidatorTests()
    {
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentAccessRestriction();
        Fixture.CustomizeRoadSegmentOutlineCategory();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentOutlineLaneCount();
        Fixture.CustomizeRoadSegmentOutlineMorphology();
        Fixture.CustomizeRoadSegmentOutlineStatus();
        Fixture.CustomizeRoadSegmentSurfaceType();
        Fixture.CustomizeRoadSegmentOutlineWidth();
        Fixture.CustomizeEuropeanRoadNumber();
        Fixture.CustomizeNationalRoadNumber();
        Fixture.CustomizeNumberedRoadNumber();
        Fixture.CustomizeOrganizationId();
        Fixture.CustomizeOrganizationName();

        Fixture.Customize<RequestedRoadSegmentEuropeanRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<EuropeanRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentNationalRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NationalRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentNumberedRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NumberedRoadNumber>();
                instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = Fixture.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());
        Fixture.CustomizePolylineM();

        Model = new AddRoadSegment
        {
            TemporaryId = Fixture.Create<RoadSegmentId>(),
            StartNodeId = new RoadNodeId(0),
            EndNodeId = new RoadNodeId(0),
            Geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>()),
            MaintenanceAuthority = Fixture.Create<OrganizationId>(),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Morphology = Fixture.Create<RoadSegmentMorphology>(),
            Status = Fixture.Create<RoadSegmentStatus>(),
            Category = Fixture.Create<RoadSegmentCategory>(),
            AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
            LeftSideStreetNameId = Fixture.Create<int?>(),
            RightSideStreetNameId = Fixture.Create<int?>(),
            Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttribute>(1).ToArray(),
            Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttribute>(1).ToArray(),
            Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>(1).ToArray()
        };
    }

    [Fact]
    public void AccessRestrictionMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.AccessRestriction, Fixture.Create<string>());
    }

    [Fact]
    public void CategoryMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Category, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void EndNodeIdMustBeZero(int value)
    {
        ShouldHaveValidationErrorFor(c => c.EndNodeId, value);
    }

    [Fact]
    public void GeometryDrawMethodMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.GeometryDrawMethod, Fixture.Create<string>());
    }

    [Fact]
    public void GeometryHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Geometry, typeof(RoadSegmentGeometryValidator));
    }

    [Fact]
    public void GeometryMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Geometry, null);
    }

    [Fact]
    public void LaneMustNotBeNull()
    {
        var data = Fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.Lanes, data);
    }

    [Fact]
    public void LanesHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Lanes, typeof(RequestedRoadSegmentLaneAttributeValidator));
    }

    [Fact]
    public void LanesMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Lanes, null);
    }

    [Fact]
    public void MorphologyMustBeNotUnknown()
    {
        ShouldHaveValidationErrorFor(c => c.Morphology, RoadSegmentMorphology.Unknown.ToString());
    }

    [Fact]
    public void MorphologyMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Morphology, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void StartNodeIdMustBeZero(int value)
    {
        ShouldHaveValidationErrorFor(c => c.StartNodeId, value);
    }

    [Fact]
    public void StatusMustBeNotUnknown()
    {
        ShouldHaveValidationErrorFor(c => c.Status, RoadSegmentStatus.Unknown.ToString());
    }

    [Fact]
    public void StatusMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Status, Fixture.Create<string>());
    }

    [Fact]
    public void SurfaceMustNotBeNull()
    {
        var data = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.Surfaces, data);
    }

    [Fact]
    public void SurfacesHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Surfaces, typeof(RequestedRoadSegmentSurfaceAttributeValidator));
    }

    [Fact]
    public void SurfacesMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Surfaces, null);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
    }

    [Fact]
    public void WidthMustNotBeNull()
    {
        var data = Fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.Widths, data);
    }

    [Fact]
    public void WidthsHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Widths, typeof(RequestedRoadSegmentWidthAttributeValidator));
    }

    [Fact]
    public void WidthsMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Widths, null);
    }
}
