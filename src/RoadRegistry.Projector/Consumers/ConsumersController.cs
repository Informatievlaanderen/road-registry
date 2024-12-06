namespace RoadRegistry.Projector.Consumers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
    using Dapper;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("consumers")]
    public class ConsumersController : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> ListConsumers(
            [FromServices] IConfiguration configuration,
            [FromServices] ProjectionOptions projectionOptions,
            CancellationToken cancellationToken = default)
        {
            var consumerStatusses = (await Task.WhenAll(
                projectionOptions.OrganizationSync.Enabled
                    ? GetConsumerStatus(configuration,
                        WellKnownConnectionNames.OrganizationConsumerProjections,
                        WellKnownSchemas.OrganizationConsumerSchema,
                        "Synchronisatie van organisatie register",
                        cancellationToken)
                    : Task.FromResult((ConsumerStatus?)null),
                projectionOptions.StreetNameSync.Enabled
                    ? GetConsumerStatus(configuration,
                        WellKnownConnectionNames.StreetNameSnapshotConsumer,
                        WellKnownSchemas.StreetNameSnapshotConsumerSchema,
                        "Consumer van straatnaam (snapshot)",
                        cancellationToken)
                    : Task.FromResult((ConsumerStatus?)null),
                projectionOptions.StreetNameSync.Enabled
                    ? GetConsumerStatus(configuration,
                        WellKnownConnectionNames.StreetNameEventConsumer,
                        WellKnownSchemas.StreetNameEventConsumerSchema,
                        "Consumer van straatnaam (event)",
                        cancellationToken)
                    : Task.FromResult((ConsumerStatus?)null)
            )).Where(x => x is not null).ToArray();

            return Ok(consumerStatusses);
        }

        private async Task<ConsumerStatus?> GetConsumerStatus(
            IConfiguration configuration,
            string connectionStringName,
            string schemaName,
            string consumerName,
            CancellationToken ct)
        {
            await using var sqlConnection = new SqlConnection(configuration.GetRequiredConnectionString(connectionStringName));

            var result = await sqlConnection.QueryFirstOrDefaultAsync<DateTimeOffset?>(
                $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{schemaName}].[{SqlServerConsumerDbContext<DbContext>.ProcessedMessageTable}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC"
                , ct);

            return new ConsumerStatus
            {
                Name = consumerName,
                LastProcessedMessage = result ?? default
            };
        }
    }
}
