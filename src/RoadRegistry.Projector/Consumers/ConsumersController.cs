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
    using Dapper;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
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
            var organizationConsumerResult = projectionOptions.OrganizationSync.Enabled
                ? GetLastProcessedMessageDateProcessed(configuration, WellKnownConnectionNames.OrganizationConsumerProjections, WellKnownSchemas.OrganizationConsumerSchema)
                : null;
            var streetNameSnapshotConsumerResult = projectionOptions.StreetNameSync.Enabled
                ? GetLastProcessedMessageDateProcessed(configuration, WellKnownConnectionNames.StreetNameSnapshotConsumer, WellKnownSchemas.StreetNameSnapshotConsumerSchema)
                : null;
            var streetNameEventConsumerResult = projectionOptions.StreetNameSync.Enabled
                ? GetLastProcessedMessageDateProcessed(configuration, WellKnownConnectionNames.StreetNameEventConsumer, WellKnownSchemas.StreetNameEventConsumerSchema)
                : null;

            await Task.WhenAll(
                organizationConsumerResult ?? Task.CompletedTask,
                streetNameSnapshotConsumerResult ?? Task.CompletedTask,
                streetNameEventConsumerResult ?? Task.CompletedTask
            );

            var statuses = new ConsumerStatus?[]
            {
                organizationConsumerResult?.Result is not null
                    ? new()
                    {
                        Name = "Synchronisatie van organisatie register",
                        LastProcessedMessage = organizationConsumerResult.Result.Value
                    }
                    : null,
                streetNameSnapshotConsumerResult?.Result is not null
                    ? new()
                    {
                        Name = "Synchronisatie van straatnaam register (snapshot)",
                        LastProcessedMessage = streetNameSnapshotConsumerResult.Result.Value
                    }
                    : null,
                streetNameEventConsumerResult?.Result is not null
                    ? new()
                    {
                        Name = "Synchronisatie van straatnaam register (event)",
                        LastProcessedMessage = streetNameEventConsumerResult.Result.Value
                    }
                    : null
            }.Where(x => x is not null).ToArray();

            return Ok(statuses);
        }

        private async Task<DateTimeOffset?> GetLastProcessedMessageDateProcessed(IConfiguration configuration, string connectionStringName, string schemaName)
        {
            await using var sqlConnection = new SqlConnection(configuration.GetRequiredConnectionString(connectionStringName));

            var result = await sqlConnection.QueryFirstOrDefaultAsync<DateTimeOffset?>(
                $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{schemaName}].[{ProcessedMessageConfiguration.TableName}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC"
                );

            return result;
        }
    }
}
