namespace RoadRegistry.Testing
{
    using Framework;

    public interface IScenarioGivenNoneStateBuilder
    {
        IScenarioWhenStateBuilder When(Message command);
    }
}
