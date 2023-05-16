namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RemoveRoadNode = RoadRegistry.BackOffice.Messages.RemoveRoadNode;

public class RemoveRoadNodeValidatorTests : ValidatorTest<RemoveRoadNode, RemoveRoadNodeValidator>
{
    public RemoveRoadNodeValidatorTests()
    {
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();

        Model = new RemoveRoadNode
        {
            Id = Fixture.Create<RoadNodeId>()
        };
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void IdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Id, value);
    }
}