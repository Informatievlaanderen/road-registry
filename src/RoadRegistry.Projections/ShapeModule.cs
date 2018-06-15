namespace RoadRegistry.Projections
{
    using System;
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ShapeModule : Module
    {
        public ShapeModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ShapeModule>();
            var projectionsConnectionString = configuration.GetConnectionString("ShapeProjections");

            services
                .AddDbContext<ShapeContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Shape, Schema.Shape);
                    }));

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ShapeContext), Schema.Shape, MigrationTables.Shape);
        }
    }
}
