namespace RoadRegistry.Wms.Projections.Framework
{
    using System;
    using System.Linq;
    using Projac.Sql;

    internal class ScenarioExpectStateBuilder : IScenarioExpectStateBuilder
    {
        private readonly SqlProjection _projection;
        private readonly object[] _givens;
        private readonly object _when;
        private readonly IExpectation[] _expectations;

        public ScenarioExpectStateBuilder(SqlProjection projection, object[] givens, object when, IExpectation[] expectations)
        {
            _projection = projection;
            _givens = givens;
            _when = when;
            _expectations = expectations;
        }

        public IScenarioExpectStateBuilder ExpectRowCount(SqlQueryStatement query, int rowCount)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                _expectations.Concat(new[] { new RowCountExpectation(query, rowCount) }).ToArray());
        }

        public IScenarioExpectStateBuilder ExpectEmptyResultSet(SqlQueryStatement query)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                _expectations.Concat(new[] { new EmptyResultSetExpectation(query)  }).ToArray());
        }

        public IScenarioExpectStateBuilder ExpectNonEmptyResultSet(SqlQueryStatement query)
        {
            if (query == null) throw new ArgumentNullException("query");
            return new ScenarioExpectStateBuilder(
                _projection,
                _givens,
                _when,
                _expectations.Concat(new[] { new NonEmptyResultSetExpectation(query) }).ToArray());
        }

        public TestSpecification Build()
        {
            return new TestSpecification(
                _projection,
                _givens,
                _when,
                _expectations);
        }
    }
}
