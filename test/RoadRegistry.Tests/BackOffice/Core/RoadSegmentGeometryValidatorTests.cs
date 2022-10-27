namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using LineString = RoadRegistry.BackOffice.Messages.LineString;

public class RoadSegmentGeometryValidatorTests : ValidatorTest<RoadSegmentGeometry, RoadSegmentGeometryValidator>
{
    public RoadSegmentGeometryValidatorTests()
    {
        Fixture.CustomizePolylineM();

        Model = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
        Model.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
    }

    [Fact]
    public void LineStringCanNotBeNull()
    {
        var data = Fixture.CreateMany<LineString>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.MultiLineString, data);
    }

    [Fact]
    public void LineStringHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.MultiLineString, typeof(LineStringValidator));
    }

    [Fact]
    public void MultiLineStringCanNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.MultiLineString, null);
    }

    [Fact]
    public void MultiLineStringCanOnlyHaveOneLineString()
    {
        var lineStrings = Fixture.CreateMany<LineString>(new Random().Next(2, 10)).ToArray();
        ShouldHaveValidationErrorFor(c => c.MultiLineString, lineStrings);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
    }
}
