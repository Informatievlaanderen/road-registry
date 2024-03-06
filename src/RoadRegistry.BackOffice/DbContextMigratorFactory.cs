namespace RoadRegistry.BackOffice;

using System;
using System.IO;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IDbContextMigratorFactory
{
    IDbContextMigrator CreateMigrator(IConfiguration configuration, ILoggerFactory loggerFactory);
}

public abstract class DbContextMigratorFactory<TContext> : IDesignTimeDbContextFactory<TContext>, IDbContextMigratorFactory
    where TContext : DbContext
{
    protected DbContextMigratorFactory(
        string connectionStringName,
        MigrationHistoryConfiguration migrationHistoryConfiguration)
    {
        _connectionStringName = connectionStringName.ThrowIfNull();
        _migrationHistoryConfiguration = migrationHistoryConfiguration.ThrowIfNull();
    }

    private readonly string _connectionStringName;
    private readonly MigrationHistoryConfiguration _migrationHistoryConfiguration;

    protected abstract TContext CreateContext(DbContextOptions<TContext> migrationContextOptions);
    protected virtual void ConfigureOptionsBuilder(DbContextOptionsBuilder<TContext> optionsBuilder) { }
    protected virtual void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions) { }

    public TContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var contextOptions = CreateOptionsBuilder(configuration).Options;

        return CreateContext(contextOptions);
    }

    public IDbContextMigrator CreateMigrator(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var contextOptions = CreateOptionsBuilder(configuration, loggerFactory).Options;

        return new DbContextMigrator<TContext>(() => CreateContext(contextOptions), loggerFactory);
    }

    private DbContextOptionsBuilder<TContext> CreateOptionsBuilder(IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString(_connectionStringName);

        var optionsBuilder = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();

                    sqlServerOptions.MigrationsHistoryTable(
                        _migrationHistoryConfiguration.Table,
                        _migrationHistoryConfiguration.Schema);

                    ConfigureSqlServerOptions(sqlServerOptions);
                }
            )
            .UseExtendedSqlServerMigrations();

        ConfigureOptionsBuilder(optionsBuilder);

        return optionsBuilder;
    }

    private DbContextOptionsBuilder<TContext> CreateOptionsBuilder(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        if (loggerFactory == null)
            throw new ArgumentNullException(nameof(loggerFactory));

        return CreateOptionsBuilder(configuration)
            .UseLoggerFactory(loggerFactory);
    }
}

public class MigrationHistoryConfiguration
{
    public string Schema { get; init; }
    public string Table { get; init; }
}
