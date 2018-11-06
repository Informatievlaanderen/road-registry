namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class AddRoadSegmentValidatorTests
    {
        public AddRoadSegmentValidatorTests()
        {
            Fixture = new Fixture();
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
            var value = new Generator<Int32>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.GeometryDrawMethod, (Shared.RoadSegmentGeometryDrawMethod)value);
        }

        [Fact]
        public void MorphologyMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentMorphology.All, candidate => candidate.ToInt32());
            var value = new Generator<Int32>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Morphology, (Shared.RoadSegmentMorphology)value);
        }

        [Fact]
        public void StatusMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentStatus.All, candidate => candidate.ToInt32());
            var value = new Generator<Int32>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Status, (Shared.RoadSegmentStatus)value);
        }

        [Fact]
        public void CategoryMustBeWithinDomain()
        {
            var acceptable = Enum
                .GetValues(typeof(Shared.RoadSegmentCategory))
                .Cast<Shared.RoadSegmentCategory>()
                .Cast<Int32>()
                .ToArray();
            var value = new Generator<Int32>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Category, (Shared.RoadSegmentCategory)value);
        }

        [Fact]
        public void AccessRestrictionMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentAccessRestriction.All, candidate => candidate.ToInt32());
            var value = new Generator<Int32>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.AccessRestriction, (Shared.RoadSegmentAccessRestriction)value);
        }

        [Fact]
        public void PartOfEuropeanRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, (Commands.RoadSegmentEuropeanRoadProperties[])null);
        }

        [Fact]
        public void PartOfEuropeanRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentEuropeanRoadProperties>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfEuropeanRoads, data);
        }

        [Fact]
        public void PartOfEuropeanRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfEuropeanRoads, typeof(RoadSegmentEuropeanRoadPropertiesValidator));
        }

        [Fact]
        public void PartOfNationalRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, (Commands.RoadSegmentNationalRoadProperties[])null);
        }

        [Fact]
        public void PartOfNationalRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentNationalRoadProperties>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNationalRoads, data);
        }

        [Fact]
        public void PartOfNationalRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfNationalRoads, typeof(RoadSegmentNationalRoadPropertiesValidator));
        }

        [Fact]
        public void PartOfNumberedRoadsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, (Commands.RoadSegmentNumberedRoadProperties[])null);
        }

        [Fact]
        public void PartOfNumberedRoadMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentNumberedRoadProperties>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.PartOfNumberedRoads, data);
        }

        [Fact]
        public void PartOfNumberedRoadsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.PartOfNumberedRoads, typeof(RoadSegmentNumberedRoadPropertiesValidator));
        }

        [Fact]
        public void LanesMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Lanes, (Commands.RoadSegmentLaneProperties[])null);
        }

        [Fact]
        public void LaneMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentLaneProperties>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Lanes, data);
        }

        [Fact]
        public void LanesHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Lanes, typeof(RoadSegmentLanePropertiesValidator));
        }

        [Fact]
        public void WidthsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Widths, (Commands.RoadSegmentWidthProperties[])null);
        }

        [Fact]
        public void WidthMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentWidthProperties>().ToArray();
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
        public void HardeningsMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Hardenings, (Commands.RoadSegmentHardeningProperties[])null);
        }

        [Fact]
        public void HardeningMustNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RoadSegmentHardeningProperties>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Hardenings, data);
        }

        [Fact]
        public void HardeningsHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Hardenings, typeof(RoadSegmentHardeningPropertiesValidator));
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePolylineM();

            var writer = new WellKnownBinaryWriter();

            var data = new Commands.AddRoadSegment
            {
                Id = new Generator<Int32>(Fixture).First(candidate => candidate >= 0),
                StartNodeId = new Generator<Int32>(Fixture).First(candidate => candidate >= 0),
                EndNodeId = new Generator<Int32>(Fixture).First(candidate => candidate >= 0),
                Geometry = writer.Write(Fixture.Create<MultiLineString>()),
                Maintainer = Fixture.Create<string>(),
                GeometryDrawMethod = Fixture.Create<Shared.RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<Shared.RoadSegmentMorphology>(),
                Status = Fixture.Create<Shared.RoadSegmentStatus>(),
                Category = Fixture.Create<Shared.RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<Shared.RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                PartOfEuropeanRoads = Fixture.CreateMany<Commands.RoadSegmentEuropeanRoadProperties>().ToArray(),
                PartOfNationalRoads = Fixture.CreateMany<Commands.RoadSegmentNationalRoadProperties>().ToArray(),
                PartOfNumberedRoads = Fixture.CreateMany<Commands.RoadSegmentNumberedRoadProperties>().ToArray(),
                Lanes = Fixture.CreateMany<Commands.RoadSegmentLaneProperties>().ToArray(),
                Widths = Fixture.CreateMany<Commands.RoadSegmentWidthProperties>().ToArray(),
                Hardenings = Fixture.CreateMany<Commands.RoadSegmentHardeningProperties>().ToArray(),
            };

            Validator.ValidateAndThrow(data);
        }
    }
}