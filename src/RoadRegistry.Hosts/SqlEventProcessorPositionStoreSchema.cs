namespace RoadRegistry.Hosts;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public class SqlEventProcessorPositionStoreSchema
{
    public SqlEventProcessorPositionStoreSchema(SqlConnectionStringBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    private readonly SqlConnectionStringBuilder _builder;

    public async Task CreateSchemaIfNotExists(string schema, CancellationToken cancellationToken = default)
    {
        var text = $@"
                IF NOT EXISTS (SELECT * FROM SYS.SCHEMAS WHERE [Name] = '{schema}')
                BEGIN
                    EXEC('CREATE SCHEMA [{schema}] AUTHORIZATION [dbo]')
                END
                IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE [Name] = 'EventProcessorPosition' AND [Type] = 'U' AND [Schema_ID] = SCHEMA_ID('{schema}'))
                BEGIN
                    CREATE TABLE [{schema}].[EventProcessorPosition]
                    (
                        [NameHash]            BINARY(32)         NOT NULL,
                        [Name]                NVARCHAR(1024)     NOT NULL,
                        [Position]            BIGINT             NOT NULL
                        CONSTRAINT PK_EventProcessorPosition     PRIMARY KEY NONCLUSTERED (NameHash)
                    )
                END";
        await using var connection = new SqlConnection(_builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(text, connection)
        {
            CommandType = CommandType.Text
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
