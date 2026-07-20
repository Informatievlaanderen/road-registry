namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Linq;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Autofac;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Integration.Schema;
using RoadRegistry.MartenMigration.Projections;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;
using RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction;
using RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad;
using RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode;
using RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment;
using RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface;
using RoadRegistry.Product.Schema;
using RoadRegistry.Projector.EventProcessors;
using RoadRegistry.Projector.Infrastructure.Configuration;
using RoadRegistry.Projector.Infrastructure.Extensions;
using RoadRegistry.Projector.Infrastructure.Modules;
using RoadRegistry.Projector.Infrastructure.Options;
using RoadRegistry.Read.Projections;
using RoadRegistry.StreetName;
using RoadRegistry.Sync.OrganizationRegistry;
using RoadRegistry.Sync.StreetNameRegistry;
using RoadRegistry.Wfs.Schema;
using RoadRegistry.Wms.Schema;

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

                        if (projectionOptions.MartenMigration.Enabled)
                        {
                            health.AddDbContextCheck<MartenMigrationContext>();
                        }

                        if (projectionOptions.Pbs.Enabled)
                        {
                            health.AddDbContextCheck<PbsContext>();
                        }

                        if (projectionOptions.WmsWfsV2.Enabled)
                        {
                            health.AddDbContextCheck<WmsWfsV2Context>();
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
                if (projectionOptions.Extract.Enabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesExtractProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesExtractProjection(batchSize, sp.GetRequiredService<ILoggerFactory>()));
                }

                if (projectionOptions.Read.Enabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesReadProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesReadProjection(batchSize, sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IStreetNameClient>()));
                }

                if (projectionOptions.Pbs.Enabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesPbsProjection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesPbsProjection(batchSize, sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IDbContextFactory<PbsContext>>()));
                }

                if (projectionOptions.WmsWfsV2.Enabled)
                {
                    var batchSize = _configuration.GetRequiredValue<int>($"{nameof(RoadNetworkChangesWmsWfsV2Projection)}:BatchSize");
                    options.AddRoadNetworkChangesProjection(new RoadNetworkChangesWmsWfsV2Projection(batchSize, sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IDbContextFactory<WmsWfsV2Context>>()));
                }
            }).Services
            .AddMartenDatabaseMigrator()
            .AddSingleton<MartenProjectionDaemonAccessor>()
            .AddHostedService<MartenProjectionsDaemonHostedService>()
            // Keeps every Marten projection resilient: if a shard pauses on an error it is periodically resumed, while
            // all other projections keep running and the host is never affected.
            .AddHostedService<MartenProjectionSupervisor>()
            ;

        if (projectionOptions.Pbs.Enabled)
        {
            services
                .AddDbContextFactory<PbsContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.PbsProjections);
                    options.UseSqlServer(connectionString, o => o
                        .EnableRetryOnFailure()
                        .UseNetTopologySuite());
                })
                .AddSingleton<IDbMigratorFactory, PbsContextMigratorFactory>()
                // One-time sync of the PBS enum-based code lists (Wegbeheerder code list is event-driven instead).
                .AddHostedService<PbsCodeListSyncService>();
        }

        if (projectionOptions.WmsWfsV2.Enabled)
        {
            services
                .AddDbContextFactory<WmsWfsV2Context>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.WmsWfsV2Projections);
                    options.UseSqlServer(connectionString, o => o
                        .EnableRetryOnFailure()
                        .UseNetTopologySuite());
                })
                .AddSingleton<IDbMigratorFactory, WmsWfsV2ContextMigratorFactory>();
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
                .AddSingleton<IDbMigratorFactory, ExtractsDbContextMigratorFactory>()
	            .AddDbContextEventProcessorServices<ExtractsEventProcessor, ExtractsDbContext>(_ =>
	            [
	                new ExtractRequestProjection(),
	                new ExtractDownloadProjection(),
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
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
