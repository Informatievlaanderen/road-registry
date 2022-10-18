namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using AddRoadNode = RoadRegistry.BackOffice.Messages.AddRoadNode;

public class AddRoadNodeValidatorTests
{
    public AddRoadNodeValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Validator = new AddRoadNodeValidator();
    }

    public Fixture Fixture { get; }

    [Fact]
    public void GeometryHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Geometry, typeof(RoadNodeGeometryValidator));
    }

    [Fact]
    public void GeometryMustNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Geometry, (RoadNodeGeometry)null);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void TemporaryIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    public AddRoadNodeValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        Fixture.CustomizePoint();

        var data = new AddRoadNode
        {
            TemporaryId = Fixture.Create<RoadNodeId>(),
            Type = Fixture.Create<RoadNodeType>(),
            Geometry = Fixture.Create<RoadNodeGeometry>()
        };

        Validator.ValidateAndThrow(data);
    }
}