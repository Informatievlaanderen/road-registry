namespace RoadRegistry.Wms.Projections.Framework
{
    using System;
    using Projac.Sql;

    internal class ScenarioWhenStateBuilder : IScenarioWhenStateBuilder
    {
        private readonly SqlProjection _projection;
        private readonly object[] _givens;
        private readonly object _when;

        public ScenarioWhenStateBuilder(SqlProjection projection, object[] givens, object when)
        {
            _projection = projection;
            _givens = givens;
            _when = when;
        }

        public IScenarioExpectStateBuilder ExpectRowCount(SqlQueryStatement query, int rowCount)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                new IExpectation[] { new RowCountExpectation(query, rowCount) });
        }

        public IScenarioExpectStateBuilder ExpectEmptyResultSet(SqlQueryStatement query)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                new IExpectation[] { new EmptyResultSetExpectation(query) });
        }

        public IScenarioExpectStateBuilder ExpectNonEmptyResultSet(SqlQueryStatement query)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                new IExpectation[] { new NonEmptyResultSetExpectation(query) });
        }
    }
}
