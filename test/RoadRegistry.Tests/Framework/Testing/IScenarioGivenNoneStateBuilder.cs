namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioGivenNoneStateBuilder
{
    IScenarioWhenStateBuilder When(Command command);
}