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

            Fixture.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.RoadNumber = Fixture.Create<EuropeanRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Ident2 = Fixture.Create<NationalRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
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
        public void TemporaryIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
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
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, (RoadSegmentEuropeanRoadAttributes[])null);
        }

        [Fact]
        public void PartOfEuropeanRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RoadSegmentEuropeanRoadAttributes>().ToArray();
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
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, (RoadSegmentNationalRoadAttributes[])null);
        }

        [Fact]
        public void PartOfNationalRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RoadSegmentNationalRoadAttributes>().ToArray();
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
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, (RoadSegmentNumberedRoadAttributes[])null);
        }

        [Fact]
        public void PartOfNumberedRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<RoadSegmentNumberedRoadAttributes>().ToArray();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Lanes, (Messages.RoadSegmentLaneAttributes[])null);
        }

        [Fact]
        public void LaneMustNotBeNull()
        {
            var data = Fixture.CreateMany<Messages.RoadSegmentLaneAttributes>().ToArray();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Widths, (Messages.RoadSegmentWidthAttributes[])null);
        }

        [Fact]
        public void WidthMustNotBeNull()
        {
            var data = Fixture.CreateMany<Messages.RoadSegmentWidthAttributes>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Widths, data);
        }

        [Fact]
        public void WidthsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Widths, typeof(RoadSegmentWidthAttributesValidator));
        }

        [Fact]
        public void SurfacesMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Surfaces, (Messages.RoadSegmentSurfaceAttributes[])null);
        }

        [Fact]
        public void SurfaceMustNotBeNull()
        {
            var data = Fixture.CreateMany<Messages.RoadSegmentSurfaceAttributes>().ToArray();
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
                TemporaryId = Fixture.Create<RoadSegmentId>(),
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
                PartOfEuropeanRoads = Fixture.CreateMany<RoadSegmentEuropeanRoadAttributes>().ToArray(),
                PartOfNationalRoads = Fixture.CreateMany<RoadSegmentNationalRoadAttributes>().ToArray(),
                PartOfNumberedRoads = Fixture.CreateMany<RoadSegmentNumberedRoadAttributes>().ToArray(),
                Lanes = Fixture.CreateMany<Messages.RoadSegmentLaneAttributes>().ToArray(),
                Widths = Fixture.CreateMany<Messages.RoadSegmentWidthAttributes>().ToArray(),
                Surfaces = Fixture.CreateMany<Messages.RoadSegmentSurfaceAttributes>().ToArray(),
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
