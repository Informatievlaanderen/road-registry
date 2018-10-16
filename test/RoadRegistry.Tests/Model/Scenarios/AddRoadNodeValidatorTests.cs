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

    public class AddRoadNodeValidatorTests
    {
        public AddRoadNodeValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new AddRoadNodeValidator(new WellKnownBinaryReader());
        }

        public Fixture Fixture { get; }

        public AddRoadNodeValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void IdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
        }

        [Fact]
        public void TypeMustBeWithinDomain()
        {
            var types = Array.ConvertAll(RoadNodeType.All, candidate => candidate.ToInt32());
            var value = new Generator<Int32>(Fixture).First(candidate => !types.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Type, (Shared.RoadNodeType)value);
        }

        [Fact]
        public void GeometryMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, (byte[])null);
        }

        [Fact]
        public void GeometryMustBePointM()
        {
            Fixture.CustomizePolylineM();

            var writer = new WellKnownBinaryWriter();
            var geometry = Fixture.Create<MultiLineString>();
            var value = writer.Write(geometry);
            
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, value);
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePointM();

            var writer = new WellKnownBinaryWriter();

            var data = new Commands.AddRoadNode
            {
                Id = new Generator<Int32>(Fixture).First(candidate => candidate >= 0),
                Type = Fixture.Create<Shared.RoadNodeType>(),
                Geometry = writer.Write(Fixture.Create<PointM>())
            };

            Validator.ValidateAndThrow(data);
        }
    }
}