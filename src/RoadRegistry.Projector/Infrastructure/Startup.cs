namespace RoadRegistry.Projector.Infrastructure;

using Asp.Versioning.ApiExplorer;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Api;
using Configuration;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Hosts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Modules;
using Options;
using Producer.Snapshot.ProjectionHost.GradeSeparatedJunction;
using Producer.Snapshot.ProjectionHost.NationalRoad;
using Producer.Snapshot.ProjectionHost.RoadNode;
using Producer.Snapshot.ProjectionHost.RoadSegment;
using Producer.Snapshot.ProjectionHost.RoadSegmentSurface;
using Product.Schema;
using Sync.OrganizationRegistry;
using Sync.StreetNameRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using EventProcessors;
using Extracts.Projections;
using Extracts.Schema;
using Hosts.Infrastructure.Extensions;
using Integration.Schema;
using MartenMigration.Projections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.Read.Projections;
using RoadRegistry.StreetName;
using Wfs.Schema;
using Wms.Schema;

public class Startup
{
    private const string DatabaseTag = "db";
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var baseUrl = _configuration.GetValue<string>("BaseUrl")?.TrimEnd('/') ?? string.Empty;
        var projectionOptions = _configuration.GetOptions<ProjectionOptions>("Projections");

        var extractEnabled = projectionOptions.Extract?.Enabled == true;
        var readEnabled = projectionOptions.Read?.Enabled == true;
        var pbsEnabled = projectionOptions.Pbs?.Enabled == true;

        var dbContextMigratorFactories = new List<IDbContextMigratorFactory> { new ExtractsDbContextMigratorFactory() };
        if (pbsEnabled)
        {
            dbContextMigratorFactories.Add(new PbsContextMigratorFactory());
        }

        services
            .ConfigureDefaultForApi<Startup>(new StartupConfigureOptions
            {
                Cors =
                {
                    Origins = _configuration
                        .GetSection("Cors")
                        .GetChildren()
                        .Select(c => c.Value!)
                        .ToArray()
                },
                Server =
                {
                    BaseUrl = baseUrl
                },
                Swagger =
                {
                    ApiInfo = (provider, description) => new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = "Basisregisters Vlaanderen Road Registry API",
                        Description = GetApiLeadingText(description),
                        Contact = new OpenApiContact
                        {
                            Name = "Digitaal Vlaanderen",
                            Email = "digitaal.vlaanderen@vlaanderen.be",
                            Url = new Uri("https://legacy.basisregisters.vlaanderen")
                        }
                    },
                    XmlCommentPaths = [typeof(Startup).GetTypeInfo().Assembly.GetName().Name ?? "RoadRegistry.Projector"]
                },
                MiddlewareHooks =
                {
                    AfterHealthChecks = health =>
                    {
                        var connectionStrings = _configuration
                            .GetSection("ConnectionStrings")
                            .GetChildren()
                            .Where(x => x.Value is not null);

                        foreach (var connectionString in connectionStrings)
                        {
                            if (connectionString.Value!.Contains("host=", StringComparison.InvariantCultureIgnoreCase))
                            {
                                health.AddNpgSql(
                                    connectionString.Value,
                                    name: $"npgsql-{connectionString.Key.ToLowerInvariant()}",
                                    tags: [DatabaseTag, "sql", "npgsql"],
                                    timeout: TimeSpan.FromSeconds(10));
                            }
                            else
                            {
                                health.AddSqlServer(
                                    connectionString.Value,
                                    name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                    tags: [DatabaseTag, "sql", "sqlserver"]);
                            }
                        }

                        if (projectionOptions.Editor.Enabled)
                        {
                            health.AddDbContextCheck<EditorContext>();
                        }

                        if (projectionOptions.Product.Enabled)
                        {
                            health.AddDbContextCheck<ProductContext>();
                        }

                        if (projectionOptions.Wfs.Enabled)
                        {
                            health.AddDbContextCheck<WfsContext>();
                        }

                        if (projectionOptions.Wms.Enabled)
                        {
                            health.AddDbContextCheck<WmsContext>();
                        }

                        if (projectionOptions.ProducerSnapshot.Enabled)
                        {
                            health.AddDbContextCheck<RoadNodeProducerSnapshotContext>();
                            health.AddDbContextCheck<RoadSegmentProducerSnapshotContext>();
                            health.AddDbContextCheck<RoadSegmentSurfaceProducerSnapshotContext>();
                            health.AddDbContextCheck<GradeSeparatedJunctionProducerSnapshotContext>();
                            health.AddDbContextCheck<NationalRoadProducerSnapshotContext>();
                        }

                        if (projectionOptions.BackOfficeProcessors.Enabled)
                        {
                            health.AddDbContextCheck<BackOfficeProcessorDbContext>();
                        }

                        if (projectionOptions.OrganizationSync.Enabled)
                        {
                            health.AddDbContextCheck<OrganizationConsumerContext>();
                        }

                        if (projectionOptions.StreetNameSync.Enabled)
                        {
                            health.AddDbContextCheck<StreetNameSnapshotConsumerContext>();
                            health.AddDbContextCheck<StreetNameSnapshotProjectionContext>();
                            health.AddDbContextCheck<StreetNameEventConsumerContext>();
                            health.AddDbContextCheck<StreetNameEventProjectionContext>();
                        }

                        if (projectionOptions.MartenMigration?.Enabled == true)
                        {
                            health.AddDbContextCheck<MartenMigrationContext>();
                        }

                        if (projectionOptions.Pbs?.Enabled == true)
                        {
                            health.Add(new HealthCheckRegistration(
                                "pbscontext",
                                sp => new PbsDbContextHealthCheck(sp.GetRequiredService<IDbContextFactory<PbsContext>>()),
                                HealthStatus.Unhealthy,
                                tags: [DatabaseTag, "sql", "sqlserver"]));
                        }
                    }
                }
            })
            .AddValidatorsFromAssemblyContaining<Startup>()
            .RegisterLoggingModule(_configuration)
            .AddSingleton(projectionOptions)

            .AddDbContext<EditorContext>(WellKnownConnectionNames.EditorProjections)
            .AddDbContext<ProductContext>(WellKnownConnectionNames.ProductProjections)
            .AddDbContext<WfsContext>(WellKnownConnectionNames.WfsProjections)
            .AddDbContext<WmsContext>(WellKnownConnectionNames.WmsProjections)
            .AddDbContext<RoadNodeProducerSnapshotContext>(WellKnownConnectionNames.ProducerSnapshotProjections)
            .AddDbContext<RoadSegmentProducerSnapshotContext>(WellKnownConnectionNames.ProducerSnapshotProjections)
            .AddDbContext<NationalRoadProducerSnapshotContext>(WellKnownConnectionNames.ProducerSnapshotProjections)
            .AddDbContext<GradeSeparatedJunctionProducerSnapshotContext>(WellKnownConnectionNames.ProducerSnapshotProjections)
            .AddDbContext<RoadSegmentSurfaceProducerSnapshotContext>(WellKnownConnectionNames.ProducerSnapshotProjections)
            .AddDbContext<BackOfficeProcessorDbContext>(WellKnownConnectionNames.Events)
            .AddDbContext<OrganizationConsumerContext>(WellKnownConnectionNames.OrganizationConsumerProjections)
            .AddDbContext<StreetNameSnapshotConsumerContext>(WellKnownConnectionNames.StreetNameSnapshotConsumer)
            .AddDbContext<StreetNameSnapshotProjectionContext>(WellKnownConnectionNames.StreetNameProjections)
            .AddDbContext<StreetNameEventConsumerContext>(WellKnownConnectionNames.StreetNameEventConsumer)
            .AddDbContext<StreetNameEventProjectionContext>(WellKnownConnectionNames.StreetNameProjections)
            .AddDbContext<IntegrationContext>(WellKnownConnectionNames.IntegrationProjections)
            .AddDbContext<MartenMigrationContext>(WellKnownConnectionNames.MartenMigration)

            .AddHttpClient()
            .AddStreetNameClient()
            .AddMartenRoad((options, sp) =>
            {
                if (extractEnabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesExtractProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesExtractProjection(batchSize, sp.GetRequiredService<ILoggerFactory>()));
                }

                if (readEnabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesReadProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesReadProjection(batchSize, sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IStreetNameClient>()));
                }

                if (pbsEnabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesPbsProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesPbsProjection(batchSize, sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IDbContextFactory<PbsContext>>()));
                }
            }).Services
            .AddMartenDatabaseMigrations()
            .AddSingleton<MartenProjectionDaemonAccessor>()
            .AddHostedService<MartenProjectionsDaemonHostedService>() // Must be after MartenDatabaseMigrations
            // Keeps every Marten projection resilient: if a shard pauses on an error it is periodically resumed, while
            // all other projections keep running and the host is never affected.
            .AddHostedService<MartenProjectionSupervisor>()

            .AddSingleton(dbContextMigratorFactories.ToArray())
            ;

        if (pbsEnabled)
        {
            services
                .AddDbContextFactory<PbsContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.PbsProjections);
                    options.UseSqlServer(connectionString, o => o
                        .EnableRetryOnFailure()
                        .UseNetTopologySuite());
                })
                // One-time sync of the PBS enum-based code lists (Wegbeheerder code list is event-driven instead).
                .AddHostedService<PbsCodeListSyncService>();
        }

        // extracts projections until GRB has been migrated
        {
	        services
	            .AddSingleton<IClock>(SystemClock.Instance)
	            .AddSingleton<Scheduler>()
	            .AddSingleton(new EnvelopeFactory(
	                ExtractsEventProcessor.EventMapping,
	                new EventDeserializer((eventData, eventType) =>
	                    JsonConvert.DeserializeObject(eventData, eventType, ExtractsEventProcessor.SerializerSettings)))
	            )
	            .AddExtractsDbContextFactory(QueryTrackingBehavior.TrackAll, WellKnownConnectionNames.ExtractsAdmin)
	            .AddDbContextEventProcessorServices<ExtractsEventProcessor, ExtractsDbContext>(_ =>
	            [
	                new RoadRegistry.Extracts.Projections.ExtractRequestProjection(),
	                new RoadRegistry.Extracts.Projections.ExtractDownloadProjection(),
	            ])
	            .AddHostedService<ExtractsEventProcessor>();
        }
    }

    public void Configure(
        IServiceProvider serviceProvider,
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IHostApplicationLifetime appLifetime,
        ILoggerFactory loggerFactory,
        IApiVersionDescriptionProvider apiVersionProvider,
        IConfiguration configuration,
        HealthCheckService healthCheckService)
    {
        StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();

        app
            .UseDefaultForApi(new StartupUseOptions
            {
                Common =
                {
                    ApplicationContainer = new ContainerBuilder().Build(),
                    ServiceProvider = serviceProvider,
                    HostingEnvironment = env,
                    ApplicationLifetime = appLifetime,
                    LoggerFactory = loggerFactory
                },
                Api =
                {
                    VersionProvider = apiVersionProvider,
                    Info = groupName =>
                        $"Basisregisters.Vlaanderen - Road Registry API {groupName}",
                    CSharpClientOptions =
                    {
                        ClassName = "RoadRegistryProjector",
                        Namespace = "Be.Vlaanderen.Basisregisters"
                    },
                    TypeScriptClientOptions =
                    {
                        ClassName = "RoadRegistryProjector"
                    }
                },
                Server =
                {
                    PoweredByName = "Vlaamse overheid - Basisregisters Vlaanderen",
                    ServerName = "Digitaal Vlaanderen"
                },
                MiddlewareHooks =
                {
                    AfterMiddleware = x => x.UseMiddleware<AddNoCacheHeadersMiddleware>()
                }
            });

        var migratorFactories = serviceProvider.GetRequiredService<IDbContextMigratorFactory[]>();

        foreach (var migratorFactory in migratorFactories)
        {
            migratorFactory.CreateMigrator(configuration, loggerFactory)
                .MigrateAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // The Marten projection daemon is started by MartenProjectionsDaemonHostedService (registered after the
        // schema migrator), so it runs after the migrations gate completes and within the host lifecycle.
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
