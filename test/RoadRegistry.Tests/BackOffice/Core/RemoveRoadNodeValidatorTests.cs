namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using RemoveRoadNode = RoadRegistry.BackOffice.Messages.RemoveRoadNode;

public class RemoveRoadNodeValidatorTests
{
    public RemoveRoadNodeValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Validator = new RemoveRoadNodeValidator();
    }

    public Fixture Fixture { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void IdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
    }

    public RemoveRoadNodeValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var data = new RemoveRoadNode
        {
            Id = Fixture.Create<RoadNodeId>()
        };

        Validator.ValidateAndThrow(data);
    }
}
