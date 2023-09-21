namespace RoadRegistry.SyncHost.Extensions;

using System;
using Autofac;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public static class AutofacExtensions
{
    public static void RegisterDbContext<TDbContext>(this ContainerBuilder builder,
        string connectionStringName,
        Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction,
        Func<DbContextOptionsBuilder<TDbContext>, TDbContext> dbContextBuilder)
        where TDbContext : DbContext, new()
    {
        builder.Register<IComponentContext, IConfiguration, TraceDbConnection<TDbContext>>(
            (context, configuration) =>
            {
                var connectionString = configuration.GetConnectionString(connectionStringName);

                return new TraceDbConnection<TDbContext>(
                    new SqlConnection(connectionString),
                    configuration["DataDog:ServiceName"]);
            });

        builder.Register<IComponentContext, IConfiguration, ILoggerFactory, TDbContext>(
            (context, configuration, loggerFactory) =>
            {
                var connectionString = configuration.GetConnectionString(connectionStringName);

                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>()
                    .UseLoggerFactory(loggerFactory);

                if (!string.IsNullOrWhiteSpace(connectionString))
                    optionsBuilder.UseSqlServer(context.Resolve<TraceDbConnection<TDbContext>>(), sqlServerOptionsAction);
                else
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { });

                return dbContextBuilder(optionsBuilder);
            }).InstancePerLifetimeScope();
    }

    public static ContainerBuilder RegisterProjectionMigrator<TContextMigrationFactory>(this ContainerBuilder builder)
        where TContextMigrationFactory : IRunnerDbContextMigratorFactory, new()
    {
        builder
            .Register<IComponentContext, IConfiguration, ILoggerFactory, IRunnerDbContextMigrator>(
                (context, configuration, loggerFactory) => new TContextMigrationFactory().CreateMigrator(configuration, loggerFactory)
            )
            .As<IRunnerDbContextMigrator>();

        return builder;
    }
}
