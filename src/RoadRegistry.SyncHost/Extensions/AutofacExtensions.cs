namespace RoadRegistry.SyncHost.Extensions;

using Autofac;
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
        builder.Register<IComponentContext, IConfiguration, ILoggerFactory, TDbContext>(
            (context, configuration, loggerFactory) =>
            {
                var connectionString = configuration.GetConnectionString(connectionStringName);

                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>()
                    .UseLoggerFactory(loggerFactory);

                if (!string.IsNullOrWhiteSpace(connectionString))
                    optionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction);
                else
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { });

                return dbContextBuilder(optionsBuilder);
            }).InstancePerLifetimeScope();

        return builder;
    }
}
