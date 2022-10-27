namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using Xunit;
using LineString = RoadRegistry.BackOffice.Messages.LineString;
using Point = RoadRegistry.BackOffice.Messages.Point;

public class LineStringValidatorTests : ValidatorTest<LineString, LineStringValidator>
{
    public LineStringValidatorTests()
    {
        Fixture.CustomizePolylineM();

        var geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
        Model = geometry.MultiLineString[new Random().Next(0, geometry.MultiLineString.Length)];
    }

    [Fact]
    public void MeasureCanNotBeNaN()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.NaN;

        ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasureCanNotBeNegativeInfinity()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.NegativeInfinity;

        ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasureCanNotBePositiveInfinity()
    {
        var data = Fixture.CreateMany<double>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = double.PositiveInfinity;

        ShouldHaveValidationErrorFor(c => c.Measures, data);
    }

    [Fact]
    public void MeasuresCanNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Measures, null);
    }

    [Fact]
    public void PointCanNotBeNull()
    {
        var data = Fixture.CreateMany<Point>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.Points, data);
    }

    [Fact]
    public void PointsCanNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Points, null);
    }
}
