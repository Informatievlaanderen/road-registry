namespace RoadRegistry.BackOffice.MessagingHost.Kafka
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using RoadRegistry.Hosts;

    public class ConsumerModule : Module
    {
        public ConsumerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ConsumerModule>();
            var connectionString = configuration.GetConnectionString(WellknownConnectionNames.StreetNameConsumer);

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(configuration, services, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string connectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<StreetNameConsumerContext>(
                    new SqlConnection(connectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<StreetNameConsumerContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<StreetNameConsumerContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.StreetNameConsumer, WellknownSchemas.StreetNameConsumerSchema);
                    }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<StreetNameConsumerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(StreetNameConsumerContext));
        }
    }
}
