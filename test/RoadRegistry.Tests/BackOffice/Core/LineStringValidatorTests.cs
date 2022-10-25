namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using Xunit;
using Point = RoadRegistry.BackOffice.Messages.Point;

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
    public void MeasureCanNotBeNaN()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.NaN;

        Validator.ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasureCanNotBeNegativeInfinity()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.NegativeInfinity;

        Validator.ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasureCanNotBePositiveInfinity()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.PositiveInfinity;

        Validator.ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasuresCanNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Measures, (double[])null);
    }

    [Fact]
    public void PointCanNotBeNull()
    {
        var data = Fixture.CreateMany<Point>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        Validator.ShouldHaveValidationErrorFor(c => c.Points, data);
    }

    [Fact]
    public void PointsCanNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Points, (Point[])null);
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