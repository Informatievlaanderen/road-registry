namespace RoadRegistry.Wms.Projections.Framework
{
    using System.Data.SqlClient;

    /// <summary>
    /// Embodies a specific expectation about the projection handling outcome.
    /// </summary>
    public interface IExpectation
    {
        /// <summary>
        /// Verifies this expectation is met.
        /// </summary>
        /// <param name="transaction">The transaction used to perform the expectation verification.</param>
        /// <returns><c>true</c> if the expectation is met, otherwise <c>false</c>.</returns>
        ExpectationVerificationResult Verify(SqlTransaction transaction);
    }
}