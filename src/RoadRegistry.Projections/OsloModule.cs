namespace RoadRegistry.Projections.Oslo
{
    using System;
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class OsloModule : Module
    {
        public OsloModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OsloModule>();
            var osloProjectionsConnectionString = configuration.GetConnectionString("OsloProjections");

            services
                .AddDbContext<OsloContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(osloProjectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Oslo, Schema.Oslo);
                    }));

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(OsloContext), Schema.Oslo, MigrationTables.Oslo);
        }
    }
}
