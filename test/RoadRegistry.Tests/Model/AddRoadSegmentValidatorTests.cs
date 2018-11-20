namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class AddRoadSegmentValidatorTests
    {
        public AddRoadSegmentValidatorTests()
        {
            Fixture = new Fixture();
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
            Fixture.CustomizeMaintenanceAuthorityId();
            Fixture.CustomizeMaintenanceAuthorityName();

            Fixture.Customize<RequestedRoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.RoadNumber =Fixture.Create<EuropeanRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident2 = Fixture.Create<NationalRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());
            Validator = new AddRoadSegmentValidator();
        }

        public Fixture Fixture { get; }

        public AddRoadSegmentValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void IdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void StartNodeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.StartNodeId, value);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void EndNodeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.EndNodeId, value);
        }

        [Fact]
        public void StartNodeMustNotBeEndNodeId()
        {
            var id = Fixture.Create<RoadNodeId>();
            var data = new Messages.AddRoadSegment
            {
                StartNodeId = id, EndNodeId = id
            };
            Validator.ShouldHaveValidationErrorFor(c => c.EndNodeId, data);
        }

        [Fact]
        public void GeometryMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, (RoadSegmentGeometry)null);
        }

        [Fact]
        public void GeometryHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Geometry, typeof(RoadSegmentGeometryValidator));
        }

        [Fact]
        public void GeometryDrawMethodMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.GeometryDrawMethod, Fixture.Create<string>());
        }

        [Fact]
        public void MorphologyMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Morphology, Fixture.Create<string>());
        }

        [Fact]
        public void StatusMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Status, Fixture.Create<string>());
        }

        [Fact]
        public void CategoryMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Category, Fixture.Create<string>());
        }

        [Fact]
        public void AccessRestrictionMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.AccessRestriction, Fixture.Create<string>());
        }

        [Fact]
        public void PartOfEuropeanRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, (RequestedRoadSegmentEuropeanRoadAttributes[])null);
        }

        [Fact]
        public void PartOfEuropeanRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, data);
        }

        [Fact]
        public void PartOfEuropeanRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfEuropeanRoads, typeof(RoadSegmentEuropeanRoadAttributesValidator));
        }

        [Fact]
        public void PartOfNationalRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, (RequestedRoadSegmentNationalRoadAttributes[])null);
        }

        [Fact]
        public void PartOfNationalRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, data);
        }

        [Fact]
        public void PartOfNationalRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfNationalRoads, typeof(RoadSegmentNationalRoadAttributesValidator));
        }

        [Fact]
        public void PartOfNumberedRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, (RequestedRoadSegmentNumberedRoadAttributes[])null);
        }

        [Fact]
        public void PartOfNumberedRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, data);
        }

        [Fact]
        public void PartOfNumberedRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfNumberedRoads, typeof(RoadSegmentNumberedRoadAttributesValidator));
        }

        [Fact]
        public void LanesMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Lanes, (RequestedRoadSegmentLaneAttributes[])null);
        }

        [Fact]
        public void LaneMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Lanes, data);
        }

        [Fact]
        public void LanesHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Lanes, typeof(RoadSegmentLaneAttributesValidator));
        }

        [Fact]
        public void WidthsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Widths, (RequestedRoadSegmentWidthAttributes[])null);
        }

        [Fact]
        public void WidthMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Widths, data);
        }

        [Fact]
        public void WidthsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Widths, typeof(RoadSegmentWidthPropertiesValidator));
        }

        [Fact]
        public void SurfacesMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Surfaces, (RequestedRoadSegmentSurfaceAttributes[])null);
        }

        [Fact]
        public void SurfaceMustNotBeNull()
        {
            var data = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Surfaces, data);
        }

        [Fact]
        public void SurfacesHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Surfaces, typeof(RoadSegmentSurfaceAttributesValidator));
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePolylineM();

            var data = new Messages.AddRoadSegment
            {
                Id = Fixture.Create<RoadSegmentId>(),
                StartNodeId = Fixture.Create<RoadNodeId>(),
                EndNodeId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>()),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                PartOfEuropeanRoads = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>().ToArray(),
                PartOfNationalRoads = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>().ToArray(),
                PartOfNumberedRoads = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>().ToArray(),
                Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray(),
                Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray(),
                Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray(),
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
