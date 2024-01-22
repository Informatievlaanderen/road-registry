namespace RoadRegistry.Projector.Consumers
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Dapper;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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
            var streetNameConsumerResult = projectionOptions.StreetNameSync.Enabled
                ? GetLastProcessedMessageDateProcessed(configuration, WellKnownConnectionNames.StreetNameSnapshotConsumer, WellKnownSchemas.StreetNameSnapshotConsumerSchema)
                : null;

            await Task.WhenAll(organizationConsumerResult ?? Task.CompletedTask, streetNameConsumerResult ?? Task.CompletedTask);

            var statuses = new ConsumerStatus?[]
            {
                organizationConsumerResult?.Result is not null
                    ? new()
                    {
                        Name = "Synchronisatie van organisatie register",
                        LastProcessedMessage = organizationConsumerResult.Result.Value
                    }
                    : null,
                streetNameConsumerResult?.Result is not null
                    ? new()
                    {
                        Name = "Synchronisatie van straatnaam register",
                        LastProcessedMessage = streetNameConsumerResult.Result.Value
                    }
                    : null
            }.Where(x => x is not null).ToArray();

            return Ok(statuses);
        }

        private async Task<DateTimeOffset?> GetLastProcessedMessageDateProcessed(IConfiguration configuration, string connectionStringName, string schemaName)
        {
            await using var sqlConnection = new SqlConnection(configuration.GetConnectionString(connectionStringName));

            var result = await sqlConnection.QueryFirstOrDefaultAsync<DateTimeOffset?>(
                $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{schemaName}].[{ProcessedMessageConfiguration.TableName}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC"
                );

            return result;
        }
    }
}
