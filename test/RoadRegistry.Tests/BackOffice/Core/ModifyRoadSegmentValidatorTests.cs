namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using ModifyRoadSegment = RoadRegistry.BackOffice.Messages.ModifyRoadSegment;
using RequestedRoadSegmentEuropeanRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentEuropeanRoadAttribute;
using RequestedRoadSegmentNationalRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNationalRoadAttribute;
using RequestedRoadSegmentNumberedRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNumberedRoadAttribute;

public class ModifyRoadSegmentValidatorTests : ValidatorTest<ModifyRoadSegment, ModifyRoadSegmentValidator>
{
    public ModifyRoadSegmentValidatorTests()
    {
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentCategory();
        Fixture.CustomizeRoadSegmentMorphology();
        Fixture.CustomizeRoadSegmentStatus();
        Fixture.CustomizeRoadSegmentAccessRestriction();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();
        Fixture.CustomizeRoadSegmentLaneCount();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentSurfaceType();
        Fixture.CustomizeRoadSegmentWidth();
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
        Fixture.CustomizeModifyRoadSegment();

        Model = Fixture.Create<ModifyRoadSegment>();
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
    public void EndNodeIdMustBeGreaterThan(int value)
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

//        [Fact]
//        public void PartOfEuropeanRoadsMustNotBeNull()
//        {
//            ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, (RoadSegmentEuropeanRoadAttributes[])null);
//        }
//
//        [Fact]
//        public void PartOfEuropeanRoadMustNotBeNull()
//        {
//            var data = Fixture.CreateMany<RoadSegmentEuropeanRoadAttributes>().ToArray();
//            var index = new Random().Next(0, data.Length);
//            data[index] = null;
//
//            ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, data);
//        }
//
//        [Fact]
//        public void PartOfEuropeanRoadsHasExpectedValidator()
//        {
//            Validator.ShouldHaveChildValidator(c => c.PartOfEuropeanRoads, typeof(ModifyRoadSegmentToEuropeanRoadValidator));
//        }
//
//        [Fact]
//        public void PartOfNationalRoadsMustNotBeNull()
//        {
//            ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, (RoadSegmentNationalRoadAttributes[])null);
//        }
//
//        [Fact]
//        public void PartOfNationalRoadMustNotBeNull()
//        {
//            var data = Fixture.CreateMany<RoadSegmentNationalRoadAttributes>().ToArray();
//            var index = new Random().Next(0, data.Length);
//            data[index] = null;
//
//            ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, data);
//        }
//
//        [Fact]
//        public void PartOfNationalRoadsHasExpectedValidator()
//        {
//            Validator.ShouldHaveChildValidator(c => c.PartOfNationalRoads, typeof(ModifyRoadSegmentToNationalRoadValidator));
//        }
//
//        [Fact]
//        public void PartOfNumberedRoadsMustNotBeNull()
//        {
//            ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, (RoadSegmentNumberedRoadAttributes[])null);
//        }
//
//        [Fact]
//        public void PartOfNumberedRoadMustNotBeNull()
//        {
//            var data = Fixture.CreateMany<RoadSegmentNumberedRoadAttributes>().ToArray();
//            var index = new Random().Next(0, data.Length);
//            data[index] = null;
//
//            ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, data);
//        }
//
//        [Fact]
//        public void PartOfNumberedRoadsHasExpectedValidator()
//        {
//            Validator.ShouldHaveChildValidator(c => c.PartOfNumberedRoads, typeof(ModifyRoadSegmentToNumberedRoadValidator));
//        }

    [Fact]
    public void LanesMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Lanes, null);
    }

    [Fact]
    public void LanesMustNotBeEmpty()
    {
        ShouldHaveValidationErrorFor(c => c.Lanes, Array.Empty<RequestedRoadSegmentLaneAttribute>());
    }

    [Fact]
    public void MorphologyMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Morphology, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void StartNodeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.StartNodeId, value);
    }

    [Fact]
    public void StartNodeMustNotBeEndNodeId()
    {
        ShouldHaveValidationErrorFor(c => c.EndNodeId, Model.StartNodeId);
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

    [Fact]
    public void SurfacesMustNotBeEmpty()
    {
        ShouldHaveValidationErrorFor(c => c.Surfaces, Array.Empty<RequestedRoadSegmentSurfaceAttribute>());
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

    [Fact]
    public void WidthsMustNotBeEmpty()
    {
        ShouldHaveValidationErrorFor(c => c.Widths, Array.Empty<RequestedRoadSegmentWidthAttribute>());
    }
}
