namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

public interface IDbContextMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}

public class DbContextMigrator<TContext> : IDbContextMigrator
    where TContext : DbContext
{
    private const int RetryCount = 5;
    private readonly Func<TContext> _createContext;
    private readonly ILogger<DbContextMigrator<TContext>> _logger;

    public DbContextMigrator(Func<TContext> createContext, ILoggerFactory loggerFactory)
    {
        _createContext = createContext;
        _logger = loggerFactory?.CreateLogger<DbContextMigrator<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(
                RetryCount,
                retryAttempt =>
                {
                    var value = Math.Pow(2, retryAttempt) / 4;
                    var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                    _logger.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                    return TimeSpan.FromSeconds(randomValue);
                }
            )
            .ExecuteAsync(
                async token =>
                {
                    _logger.LogInformation("Running EF Migrations for {ContextType}", typeof(TContext).Name);
                    using (var migrationContext = _createContext())
                    {
                        await migrationContext.MigrateAsync(cancellationToken);
                    }
                },
                cancellationToken
            );
    }
}
