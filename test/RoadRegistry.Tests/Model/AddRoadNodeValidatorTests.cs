namespace RoadRegistry.Model
{
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
            Fixture.CustomizeRoadNodeId();
            Fixture.CustomizeRoadNodeType();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
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
        public void GeometryMustBeWellformed()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry, Fixture.CreateMany<byte>().ToArray());
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePointM();

            var writer = new WellKnownBinaryWriter();

            var data = new Messages.AddRoadNode
            {
                Id = Fixture.Create<RoadNodeId>(),
                Type = Fixture.Create<RoadNodeType>(),
                Geometry = writer.Write(Fixture.Create<PointM>())
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
