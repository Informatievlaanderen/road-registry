namespace RoadRegistry.Wms.Projections.Framework
{
    /// <summary>
    /// The when state within the test specification building process.
    /// </summary>
    public interface IScenarioWhenStateBuilder
    {
        /// <summary>
        /// Expect the query to return the row count as a scalar.
        /// </summary>
        /// <param name="query">The count query to execute.</param>
        /// <param name="rowCount">The expected row count.</param>
        /// <returns>A builder continuation.</returns>
        IScenarioExpectStateBuilder ExpectRowCount(SqlQueryStatement query, int rowCount);

        /// <summary>
        /// Expect the query to return an empty resultset.
        /// </summary>
        /// <param name="query">The count query to execute.</param>
        /// <returns>A builder continuation.</returns>
        IScenarioExpectStateBuilder ExpectEmptyResultSet(SqlQueryStatement query);

        /// <summary>
        /// Expect the query to return a non empty resultset.
        /// </summary>
        /// <param name="query">The count query to execute.</param>
        /// <returns>A builder continuation.</returns>
        IScenarioExpectStateBuilder ExpectNonEmptyResultSet(SqlQueryStatement query);
    }
}
