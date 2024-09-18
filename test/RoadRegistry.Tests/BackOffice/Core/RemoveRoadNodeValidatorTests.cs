namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice.Core;
using RemoveRoadNode = RoadRegistry.BackOffice.Messages.RemoveRoadNode;

public class RemoveRoadNodeValidatorTests : ValidatorTest<RemoveRoadNode, RemoveRoadNodeValidator>
{
    public RemoveRoadNodeValidatorTests()
    {
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Fixture.CustomizeRemoveRoadNode();

        Model = Fixture.Create<RemoveRoadNode>();
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
