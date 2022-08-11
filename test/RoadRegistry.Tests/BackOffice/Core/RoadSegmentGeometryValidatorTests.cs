namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using Xunit;
using LineString = Messages.LineString;

public class RoadSegmentGeometryValidatorTests
{
    public RoadSegmentGeometryValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizePolylineM();

        Validator = new RoadSegmentGeometryValidator();
    }

    public Fixture Fixture { get; }

    public RoadSegmentGeometryValidator Validator { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
    }

    [Fact]
    public void MultiLineStringCanNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.MultiLineString, (LineString[])null);
    }

    [Fact]
    public void MultiLineStringCanOnlyHaveOneLineString()
    {
        var lineStrings = Fixture.CreateMany<LineString>(new Random().Next(2, 10)).ToArray();
        Validator.ShouldHaveValidationErrorFor(c => c.MultiLineString, lineStrings);
    }

    [Fact]
    public void LineStringCanNotBeNull()
    {
        var data = Fixture.CreateMany<LineString>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        Validator.ShouldHaveValidationErrorFor(c => c.MultiLineString, data);
    }

    [Fact]
    public void LineStringHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.MultiLineString, typeof(LineStringValidator));
    }

    [Fact]
    public void VerifyValid()
    {
        var data = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
        data.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();

        Validator.ValidateAndThrow(data);
    }
}
