namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Hands out contexts scoped to a shadow schema. The live read model is registered through AddDbContextFactory, which
// is keyed by context type and so can only carry one schema; the shadow one is built here instead, from the same
// connection string and the same options.
public sealed class TempSchemaDbContextFactory<TContext> : IDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly Func<TContext> _createDbContext;

    public TempSchemaDbContextFactory(Func<TContext> createDbContext)
    {
        _createDbContext = createDbContext;
    }

    public TContext CreateDbContext()
    {
        return _createDbContext();
    }
}

// Creates a shadow schema and its tables when they are not there yet.
//
// The migrations own the production schema; the shadow copy is not migrated, it is created from the model the shadow
// projection is about to write through - so it matches that model by construction, including whatever the migrations
// have added since. It runs before the Marten daemon starts, and does nothing at all once the tables exist.
public sealed class TempSchemaBootstrapper : IHostedService
{
    private readonly IReadOnlyList<(string Schema, Func<DbContext> CreateDbContext)> _readModels;
    private readonly ILogger<TempSchemaBootstrapper> _logger;

    public TempSchemaBootstrapper(
        IReadOnlyList<(string Schema, Func<DbContext> CreateDbContext)> readModels,
        ILogger<TempSchemaBootstrapper> logger)
    {
        _readModels = readModels;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var (schema, createDbContext) in _readModels)
        {
            try
            {
                await CreateWhenMissing(schema, createDbContext, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The shadow projection will fail on its first batch and pause; the host and the live projections must
                // not be taken down over a read model that only exists to be swapped in later.
                _logger.LogError(ex, "Could not create the {Schema} shadow read model.", schema);
            }
        }
    }

    private async Task CreateWhenMissing(string schema, Func<DbContext> createDbContext, CancellationToken cancellationToken)
    {
        await using var context = createDbContext();

        var tables = await context.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = {schema}")
            .SingleAsync(cancellationToken);
        if (tables > 0)
        {
            _logger.LogInformation("The {Schema} shadow read model already has {Tables} table(s); leaving it alone.", schema, tables);
            return;
        }

        _logger.LogWarning("Creating the {Schema} shadow read model from the projection's model.", schema);

        // Creates the schema and every table in the model, and nothing else - not the database, and not the
        // migrations history table, which belongs to the schema the migrations own.
        var databaseCreator = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();
        await databaseCreator.CreateTablesAsync(cancellationToken);

        _logger.LogWarning("The {Schema} shadow read model was created.", schema);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
