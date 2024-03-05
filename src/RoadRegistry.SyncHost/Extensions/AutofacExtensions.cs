namespace RoadRegistry.SyncHost.Extensions;

using Autofac;
using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

public static class AutofacExtensions
{
    public static ContainerBuilder RegisterDbContext<TDbContext>(this ContainerBuilder builder,
        string connectionStringName,
        Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction,
        Func<DbContextOptionsBuilder<TDbContext>, TDbContext> dbContextBuilder)
        where TDbContext : DbContext, new()
    {
        builder.Register<IComponentContext, IConfiguration, TraceDbConnection<TDbContext>>(
            (context, configuration) =>
            {
                var connectionString = configuration.GetRequiredConnectionString(connectionStringName);

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

        return builder;
    }

    public static ContainerBuilder RegisterProjectionMigrator<TContextMigrationFactory>(this ContainerBuilder builder)
        where TContextMigrationFactory : IDbContextMigratorFactory, new()
    {
        builder
            .Register<IComponentContext, IConfiguration, ILoggerFactory, IDbContextMigrator>(
                (context, configuration, loggerFactory) => new TContextMigrationFactory().CreateMigrator(configuration, loggerFactory)
            )
            .As<IDbContextMigrator>();

        return builder;
    }
}
