namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;

public class ModifyRoadNodeValidatorTests : ValidatorTest<ModifyRoadNode, ModifyRoadNodeValidator>
{
    public ModifyRoadNodeValidatorTests()
    {
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Fixture.CustomizePoint();

        Model = new ModifyRoadNode
        {
            Id = Fixture.Create<RoadNodeId>(),
            Type = Fixture.Create<RoadNodeType>(),
            Geometry = Fixture.Create<RoadNodeGeometry>()
        };
    }

    [Fact]
    public void GeometryHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Geometry, typeof(RoadNodeGeometryValidator));
    }

    [Fact]
    public void GeometryMustNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Geometry, null);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void IdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Id, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }
}
