namespace RoadRegistry.Api.Modules
{
    using System;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections;
    using RoadRegistry.Infrastructure;

    public class ExtractShapeModule : Module
    {
        public ExtractShapeModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ShapeModule>();
            var projectionsConnectionString = configuration.GetConnectionString("ShapeProjections");

            services
                .AddDbContext<ShapeContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions =>
                    {
//                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Shape, Schema.ProjectionMetaData);
                    }));

            logger.LogInformation(
                "Added {Context} to services:" + Environment.NewLine +
                "\tSchema: {Schema}" + Environment.NewLine +
                "\tMigrationTable: {ProjectionMetaData}.{TableName}",
                nameof(ShapeContext),
                Schema.Shape,
                Schema.ProjectionMetaData, MigrationTables.Shape);
        }
    }
}
