namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
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
            Fixture.Customize<RequestedRoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.RoadNumber =
                        EuropeanRoadNumber.All[new Random().Next(0, EuropeanRoadNumber.All.Length)]
                            .ToString();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident2 =
                        NationalRoadNumber.All[new Random().Next(0, NationalRoadNumber.All.Length)]
                           .ToString();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident8 =
                        NumberedRoadNumber.All[new Random().Next(0, NumberedRoadNumber.All.Length)]
                            .ToString();
                    instance.Direction = Fixture.Create<NumberedRoadSegmentDirection>();
                    instance.Ordinal = new Generator<int>(Fixture).First(candidate => candidate >= 0);
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<decimal>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = new Generator<int>(Fixture)
                        .First(candidate => candidate >= 0 || candidate == -8 || candidate == -9);
                    instance.Direction = Fixture.Create<LaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<decimal>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = new Generator<int>(Fixture)
                        .First(candidate => candidate >= 0 || candidate == -8 || candidate == -9);
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<decimal>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<SurfaceType>();
                }).OmitAutoProperties());
            Validator = new AddRoadSegmentValidator(new WellKnownBinaryReader());
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
        public void GeometryMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, (byte[])null);
        }

        [Fact]
        public void GeometryMustBePolyLineM()
        {
            Fixture.CustomizePointM();

            var writer = new WellKnownBinaryWriter();
            var geometry = Fixture.Create<PointM>();
            var value = writer.Write(geometry);

            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, value);
        }

        [Fact]
        public void GeometryMustBeWellformed()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, Fixture.CreateMany<byte>().ToArray());
        }

        [Fact]
        public void GeometryDrawMethodMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentGeometryDrawMethod.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.GeometryDrawMethod, (Messages.RoadSegmentGeometryDrawMethod)value);
        }

        [Fact]
        public void MorphologyMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentMorphology.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Morphology, (Messages.RoadSegmentMorphology)value);
        }

        [Fact]
        public void StatusMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentStatus.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Status, (Messages.RoadSegmentStatus)value);
        }

        [Fact]
        public void CategoryMustBeWithinDomain()
        {
            var acceptable = Enum
                .GetValues(typeof(Messages.RoadSegmentCategory))
                .Cast<Messages.RoadSegmentCategory>()
                .Cast<int>()
                .ToArray();
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Category, (Messages.RoadSegmentCategory)value);
        }

        [Fact]
        public void AccessRestrictionMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentAccessRestriction.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.AccessRestriction, (Messages.RoadSegmentAccessRestriction)value);
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

            var writer = new WellKnownBinaryWriter();

            var data = new Messages.AddRoadSegment
            {
                Id = new Generator<int>(Fixture).First(candidate => candidate >= 0),
                StartNodeId = new Generator<int>(Fixture).First(candidate => candidate >= 0),
                EndNodeId = new Generator<int>(Fixture).First(candidate => candidate >= 0),
                Geometry = writer.Write(Fixture.Create<MultiLineString>()),
                Maintainer = Fixture.Create<string>(),
                GeometryDrawMethod = Fixture.Create<Messages.RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<Messages.RoadSegmentMorphology>(),
                Status = Fixture.Create<Messages.RoadSegmentStatus>(),
                Category = Fixture.Create<Messages.RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<Messages.RoadSegmentAccessRestriction>(),
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
