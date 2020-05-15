namespace RoadRegistry.Wms.Projections.Framework
{
    using System;
    using Projac.Sql;

    internal class ScenarioGivenNoneStateBuilder : IScenarioGivenNoneStateBuilder
    {
        private readonly SqlProjection _projection;

        public ScenarioGivenNoneStateBuilder(SqlProjection projection)
        {
            _projection = projection;
        }

        public IScenarioWhenStateBuilder When(object @event)
        {
            if (@event == null) throw new ArgumentNullException("event");
            return new ScenarioWhenStateBuilder(_projection, new object[0], @event);
        }
    }
}
