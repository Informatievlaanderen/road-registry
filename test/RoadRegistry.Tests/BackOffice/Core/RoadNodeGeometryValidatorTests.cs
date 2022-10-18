namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using Point = RoadRegistry.BackOffice.Messages.Point;

public class RoadNodeGeometryValidatorTests
{
    public RoadNodeGeometryValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizePolylineM();

        Validator = new RoadNodeGeometryValidator();
    }

    public Fixture Fixture { get; }

    [Fact]
    public void PointCanNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Point, (Point)null);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
    }

    public RoadNodeGeometryValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        Fixture.CustomizePoint();

        var data = GeometryTranslator.Translate(Fixture.Create<NetTopologySuite.Geometries.Point>());
        data.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();

        Validator.ValidateAndThrow(data);
    }
}
