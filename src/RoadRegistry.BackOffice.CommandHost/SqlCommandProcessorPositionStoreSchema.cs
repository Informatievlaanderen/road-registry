namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlCommandProcessorPositionStoreSchema
    {
        private readonly SqlConnectionStringBuilder _builder;

        public SqlCommandProcessorPositionStoreSchema(SqlConnectionStringBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public async Task CreateSchemaIfNotExists(string schema, CancellationToken cancellationToken = default)
        {
            var text = $@"
                IF NOT EXISTS (SELECT * FROM SYS.SCHEMAS WHERE [Name] = '{schema}')
                BEGIN
                    EXEC('CREATE SCHEMA [{schema}] AUTHORIZATION [dbo]')
                END
                IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE [Name] = 'CommandProcessorPosition' AND [Type] = 'U' AND [Schema_ID] = SCHEMA_ID('{schema}'))
                BEGIN
                    CREATE TABLE [{schema}].[CommandProcessorPosition]
                    (
                        [NameHash]            BINARY(32)         NOT NULL,
                        [Name]                NVARCHAR(1024)     NOT NULL,
                        [Version]             INT                NOT NULL
                        CONSTRAINT PK_CommandProcessorPosition   PRIMARY KEY NONCLUSTERED (NameHash)
                    )
                END";
            await using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using (var command = new SqlCommand(
                                 text, connection)
                             {
                                 CommandType = CommandType.Text
                             })
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
    }
}
