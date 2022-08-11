namespace RoadRegistry.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

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

    public RemoveRoadNodeValidator Validator { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void IdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
    }

    [Fact]
    public void VerifyValid()
    {
        var data = new Messages.RemoveRoadNode
        {
            Id = Fixture.Create<RoadNodeId>()
        };

        Validator.ValidateAndThrow(data);
    }
}
