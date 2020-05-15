namespace RoadRegistry.Wms.Projections.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Projac.Sql;

    internal class ScenarioGivenStateBuilder : IScenarioGivenStateBuilder
    {
        private readonly SqlProjection _projection;
        private readonly object[] _givens;

        public ScenarioGivenStateBuilder(SqlProjection projection, object[] givens)
        {
            _projection = projection;
            _givens = givens;
        }

        public IScenarioGivenStateBuilder Given(params object[] events)
        {
            if (events == null) throw new ArgumentNullException("events");
            return new ScenarioGivenStateBuilder(_projection, _givens.Concat(events).ToArray());
        }

        public IScenarioGivenStateBuilder Given(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException("events");
            return new ScenarioGivenStateBuilder(_projection, _givens.Concat(events).ToArray());
        }

        public IScenarioWhenStateBuilder When(object @event)
        {
            if (@event == null) throw new ArgumentNullException("event");
            return new ScenarioWhenStateBuilder(_projection, _givens, @event);
        }
    }
}
