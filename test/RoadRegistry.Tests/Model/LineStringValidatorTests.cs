namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class LineStringValidatorTests
    {
        public LineStringValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizePolylineM();

            Validator = new LineStringValidator();
        }

        public Fixture Fixture { get; }

        public LineStringValidator Validator { get; }

        [Fact]
        public void PointsCanNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Points, (Messages.PointWithM[])null);
        }

        [Fact]
        public void PointCanNotBeNull()
        {
            var data = Fixture.CreateMany<Messages.PointWithM>(10).ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Points, data);
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePolylineM();

            var geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
            var data = geometry.MultiLineString[new Random().Next(0, geometry.MultiLineString.Length)];

            Validator.ValidateAndThrow(data);
        }
    }
}