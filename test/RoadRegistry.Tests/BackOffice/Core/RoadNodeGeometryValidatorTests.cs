namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeGeometryValidatorTests : ValidatorTest<RoadNodeGeometry, RoadNodeGeometryValidator>
{
    public RoadNodeGeometryValidatorTests()
    {
        Fixture.CustomizePolylineM();
        Fixture.CustomizePoint();

        Model = GeometryTranslator.Translate(Fixture.Create<Point>());
        Model.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
    }

    [Fact]
    public void PointCanNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Point, null);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
    }
}