namespace RoadRegistry.Hosts;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public class SqlEventProcessorPositionStore : IEventProcessorPositionStore
{
    private readonly SqlConnectionStringBuilder _builder;
    private readonly SqlCommandText _text;

    public SqlEventProcessorPositionStore(SqlConnectionStringBuilder builder, string schema)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _text = new SqlCommandText(schema);
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


    public async Task<long?> ReadPosition(string name, CancellationToken cancellationToken)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        if (name.Length > 1024) throw new ArgumentException("The name can not be longer than 1024 characters.", nameof(name));

        var nameParameter = CreateSqlParameter(
            "@Name",
            SqlDbType.NVarChar,
            1024,
            name);

        await using var connection = new SqlConnection(_builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(_text.ReadPosition(), connection)
        {
            CommandType = CommandType.Text,
            Parameters = { nameParameter }
        };
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? default(long?) : (long)result;
    }

    private sealed class SqlCommandText
    {
        private readonly string _schema;

        public SqlCommandText(string schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public string ReadPosition()
        {
            return $"SELECT [Position] FROM [{_schema}].[EventProcessorPosition] WHERE [NameHash] = HashBytes('SHA2_256', @Name)";
        }

        public string WritePosition()
        {
            return $@"IF EXISTS (SELECT * FROM [{_schema}].[EventProcessorPosition] WITH (UPDLOCK) WHERE [NameHash] = HashBytes('SHA2_256', @Name))
    UPDATE [{_schema}].[EventProcessorPosition] SET [Position] = @Position WHERE [NameHash] = HashBytes('SHA2_256', @Name)
ELSE
    INSERT INTO [{_schema}].[EventProcessorPosition] ([NameHash], [Name], [Position]) VALUES (HashBytes('SHA2_256', @Name), @Name, @Position)";
        }
    }

    public async Task WritePosition(string name, long position, CancellationToken cancellationToken)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        if (name.Length > 1024) throw new ArgumentException("The name can not be longer than 1024 characters.", nameof(name));

        var nameParameter = CreateSqlParameter(
            "@Name",
            SqlDbType.NVarChar,
            1024,
            name);
        var positionParameter = CreateSqlParameter(
            "@Position",
            SqlDbType.BigInt,
            8,
            position);
        await using var connection = new SqlConnection(_builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);
        await using (var command = new SqlCommand(_text.WritePosition(), connection, transaction)
                     {
                         CommandType = CommandType.Text,
                         Parameters = { nameParameter, positionParameter }
                     })
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        transaction.Commit();
    }
}