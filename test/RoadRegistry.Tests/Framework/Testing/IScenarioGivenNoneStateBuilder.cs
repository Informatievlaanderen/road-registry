namespace RoadRegistry.Framework.Testing
{
    using BackOffice.Framework;

    public interface IScenarioGivenNoneStateBuilder
    {
        IScenarioWhenStateBuilder When(Command command);
    }
}
