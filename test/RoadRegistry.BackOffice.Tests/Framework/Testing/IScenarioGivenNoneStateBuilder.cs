namespace RoadRegistry.BackOffice.Framework.Testing
{
    public interface IScenarioGivenNoneStateBuilder
    {
        IScenarioWhenStateBuilder When(Command command);
    }
}
