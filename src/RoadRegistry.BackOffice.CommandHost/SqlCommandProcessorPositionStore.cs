namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlCommandProcessorPositionStore : ICommandProcessorPositionStore
    {
        private readonly SqlConnectionStringBuilder _builder;
        private readonly SqlCommandText _text;

        public SqlCommandProcessorPositionStore(SqlConnectionStringBuilder builder, string schema)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _text = new SqlCommandText(schema);
        }

        public async Task<int?> ReadVersion(string name, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length > 1024) throw new ArgumentException("The name can not be longer than 1024 characters.", nameof(name));
            var nameParameter = CreateSqlParameter(
                "@Name",
                SqlDbType.NVarChar,
                1024,
                name);

            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(_text.ReadVersion(), connection)
                {
                    CommandType = CommandType.Text,
                    Parameters = {nameParameter}
                })
                {
                    var result = await command.ExecuteScalarAsync(cancellationToken);
                    return result == null || result == DBNull.Value ? default(int?) : (int) result;
                }
            }
        }

        public async Task WriteVersion(string name, int version, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length > 1024) throw new ArgumentException("The name can not be longer than 1024 characters.", nameof(name));
            var nameParameter = CreateSqlParameter(
                "@Name",
                SqlDbType.NVarChar,
                1024,
                name);
            var versionParameter = CreateSqlParameter(
                "@Version",
                SqlDbType.Int,
                4,
                version);
            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    using (var command = new SqlCommand(_text.WriteVersion(), connection, transaction)
                    {
                        CommandType = CommandType.Text,
                        Parameters = {nameParameter, versionParameter}
                    })
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }

                    transaction.Commit();
                }
            }
        }

        private static SqlParameter CreateSqlParameter(string name, SqlDbType sqlDbType, int size, object value)
        {
            return new SqlParameter(
                name,
                sqlDbType,
                size,
                ParameterDirection.Input,
                false,
                0,
                0,
                "",
                DataRowVersion.Default,
                value);
        }

        private class SqlCommandText
        {
            private readonly string _schema;

            public SqlCommandText(string schema)
            {
                _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            }

            public string ReadVersion() =>
                $"SELECT [Version] FROM [{_schema}].[CommandProcessorPosition] WHERE [NameHash] = HashBytes('SHA2_256', @Name)";

            public string WriteVersion() =>
                $@"IF EXISTS (SELECT * FROM [{_schema}].[CommandProcessorPosition] WITH (UPDLOCK) WHERE [NameHash] = HashBytes('SHA2_256', @Name))
    UPDATE [{_schema}].[CommandProcessorPosition] SET [Version] = @Version WHERE [NameHash] = HashBytes('SHA2_256', @Name)
ELSE
    INSERT INTO [{_schema}].[CommandProcessorPosition] ([NameHash], [Name], [Version]) VALUES (HashBytes('SHA2_256', @Name), @Name, @Version)";
        }
    }
}
