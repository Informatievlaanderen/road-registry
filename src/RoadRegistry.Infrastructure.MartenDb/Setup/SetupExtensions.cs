namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using System.IO;
using System.Linq;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment;
using RoadRegistry.ScopedRoadNetwork;
using Weasel.Core;

public static class SetupExtensions
{
    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMartenRoad(this IServiceCollection services, Action<StoreOptions>? configure = null)
    {
        return AddMartenRoad(services, (options, _) =>
        {
            configure?.Invoke(options);
        });
    }

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMartenRoad(this IServiceCollection services, Action<StoreOptions, IServiceProvider>? configure = null)
    {
        services.AddSingleton<IRoadNetworkRepository, RoadNetworkRepository>();

        return services
            .AddMarten(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetRequiredConnectionString(WellKnownConnectionNames.Marten);

                var options = new StoreOptions();
                options.Connection(new NpgsqlDataSourceBuilder(connectionString)
                    .UseNetTopologySuite()
                    .Build());
                options.ConfigureRoad(configuration.GetValue<TimeSpan?>("Marten:StaleSequenceThreshold"));
                options.ConfigureGeneratedCode(sp.GetService<IHostEnvironment>());
                configure?.Invoke(options, sp);

                return options;
            });
    }

    // How Marten gets the code it generates for storage, event handling and projections (GAWR-7236). Generating it at
    // runtime means compiling it with Roslyn on the first use of every document type and projection, which is slow and
    // memory hungry at startup; the build pipeline therefore pre-generates it with `codegen write` into the host's
    // Internal/Generated folder, where it is compiled into the host's own assembly.
    //
    // Outside development the pre-built types are used when they are there, and generated as before when they are not
    // (Auto): a host whose build did not pre-generate still starts. Development always generates, so a model change is
    // never answered by stale code left over from an earlier `codegen write`. Writing generated source back to disk is
    // a development convenience only - a container has no business changing its own files.
    public static void ConfigureGeneratedCode(this StoreOptions options, IHostEnvironment? environment)
    {
        var isDevelopment = environment?.IsDevelopment() ?? false;

        options.GeneratedCodeMode = isDevelopment ? TypeLoadMode.Dynamic : TypeLoadMode.Auto;
        options.SourceCodeWritingEnabled = isDevelopment;

        // Where `codegen write` puts the files, which is where they have to be to get compiled into the host: its own
        // project folder. Left to the default, that is the content root, and it differs per host - the hosts built on
        // RoadRegistryHostBuilder use a plain HostBuilder, whose content root is the bin folder, so their code landed
        // where no build would pick it up. Nor is the working directory any better: `dotnet run --project` keeps the
        // one it was started from. The project folder is the nearest one up from the assembly holding a .csproj.
        //
        // A deployed host has no .csproj above it and keeps the default; it never writes code anyway (see above).
        var projectDirectory = FindProjectDirectory(AppContext.BaseDirectory);
        if (projectDirectory is not null)
        {
            options.GeneratedCodeOutputPath = Path.Combine(projectDirectory, "Internal", "Generated");
        }
    }

    private static string? FindProjectDirectory(string startDirectory)
    {
        try
        {
            for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
            {
                if (directory.EnumerateFiles("*.csproj").Any())
                {
                    return directory.FullName;
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // A deployed host walking up into a folder it may not read has no project folder to find either.
        }

        return null;
    }

    // How long Marten waits before it decides that a gap in mt_events.seq_id is permanent and moves the high water
    // mark past it. The sequence is handed out by nextval at INSERT time, inside the transaction, so a write that is
    // slow to commit leaves a gap that looks exactly like one left behind by a transaction that rolled back. Marten
    // tells the two apart by time alone, and its default of three seconds is nowhere near what one commit takes here:
    // the lambda appends a whole change set and runs the inline topology projection in that same transaction.
    //
    // Once the mark has jumped a gap, the events that commit late land below it, and the async daemon only ever reads
    // upwards from its position - so no projection would see them, then or ever.
    //
    // This is defence in depth, not the fix for anything observed: the loss of 2026-10-08 was the unbounded tail fetch
    // in RoadNetworkChangesProjection, and Marten cannot currently skip here at all, because the skip runs through
    // mt_mark_progression_with_skip and neither that function nor mt_high_water_skips exists in our schema (they are
    // created at runtime, which AutoCreate.None blocks, and Marten's model does not declare them so the migration
    // generator never emitted them either). Should those objects ever be added, this keeps the default of three
    // seconds - far below one inwinning commit - from turning a slow transaction into silent data loss.
    //
    // What it buys is a stall instead of a loss: a gap that really is permanent holds the async projections back for
    // this long before Marten skips it. That is the right way round - a stall is visible and heals itself, a skip is
    // silent and forever.
    public static readonly TimeSpan DefaultStaleSequenceThreshold = TimeSpan.FromMinutes(5);

    public static void ConfigureRoad(this StoreOptions options, TimeSpan? staleSequenceThreshold = null)
    {
        options.DatabaseSchemaName = WellKnownSchemas.MartenEventStore;

        options.Projections.StaleSequenceThreshold = staleSequenceThreshold ?? DefaultStaleSequenceThreshold;

        options.AutoCreateSchemaObjects = AutoCreate.None;

        options.ConfigureSerializer();

        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;
        options.Events.MetadataConfig.HeadersEnabled = true;

        // An event that cannot be deserialized must stop the projection, not be skipped. Marten's default is to
        // dead-letter it, log a warning and carry on - which advances the progression past an event that was never
        // applied, so the read models silently miss a change and no rebuild is triggered. A change to road segment
        // 818746 was lost exactly this way: RoadSegmentWasModified carries a null status for an attribute-only edit,
        // the status converter threw on it, and all four projections reported themselves fully caught up.
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.RebuildErrors.SkipSerializationErrors = false;

        options.Schema.For<IdempotentSession>()
            .DatabaseSchemaName(WellKnownSchemas.MartenEventStore)
            .Identity(x => x.Id);
    }

    public static StoreOptions ConfigureSerializer(this StoreOptions options)
    {
        options.UseNewtonsoftForSerialization(
            enumStorage: EnumStorage.AsString,
            casing: Casing.CamelCase,
            nonPublicMembersStorage: NonPublicMembersStorage.All,
            configure: settings =>
            {
                settings.ConfigureForMarten();
            });
        return options;
    }

    public static StoreOptions AddRoadNetworkTopologyProjection(this StoreOptions options)
    {
        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline, opts => opts.BatchSize = 5000);
        return options;
    }
    public static StoreOptions AddRoadAggregatesSnapshots(this StoreOptions options)
    {
        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeJunction>(SnapshotLifecycle.Inline);

        return options;
    }
}
