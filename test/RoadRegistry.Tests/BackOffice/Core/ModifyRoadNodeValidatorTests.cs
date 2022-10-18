namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;

public class ModifyRoadNodeValidatorTests
{
    public ModifyRoadNodeValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Validator = new ModifyRoadNodeValidator();
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
    public void IdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    public ModifyRoadNodeValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        Fixture.CustomizePoint();

        var data = new ModifyRoadNode
        {
            Id = Fixture.Create<RoadNodeId>(),
            Type = Fixture.Create<RoadNodeType>(),
            Geometry = Fixture.Create<RoadNodeGeometry>()
        };

        Validator.ValidateAndThrow(data);
    }
}