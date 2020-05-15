namespace RoadRegistry.Wms.Projections.Framework
{
    using System.Data;
    using System.Data.SqlClient;

    class RowCountExpectation : IExpectation
    {
        private readonly SqlQueryStatement _query;
        private readonly int _rowCount;

        public RowCountExpectation(SqlQueryStatement query, int rowCount)
        {
            _query = query;
            _rowCount = rowCount;
        }

        public ExpectationVerificationResult Verify(SqlTransaction transaction)
        {
            using (var command = new SqlCommand())
            {
                command.Connection = transaction.Connection;
                command.Transaction = transaction;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(_query.Parameters);
                command.CommandText = _query.Text;

                var result = (int)command.ExecuteScalar();
                if (result.Equals(_rowCount))
                {
                    return new RowCountExpectationVerificationPassResult(this);
                }
                return new RowCountExpectationVerificationFailResult(this, result);
            }
        }
    }
}
