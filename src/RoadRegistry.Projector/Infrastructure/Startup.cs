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
using Microsoft.OpenApi.Models;
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
using System.Linq;
using System.Reflection;
using System.Threading;
using Extracts.Schema;
using Integration.Schema;
using Wfs.Schema;
using Wms.Schema;

/// <summary>Represents the startup process for the application.</summary>
public class Startup
{
    private const string DatabaseTag = "db";
    private readonly IConfiguration _configuration;
    private IContainer _applicationContainer;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
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
                    ApplicationContainer = _applicationContainer,
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
    }

    /// <summary>Configures services for the application.</summary>
    /// <param name="services">The collection of services to configure the application with.</param>
    public IServiceProvider ConfigureServices(IServiceCollection services)
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
                    XmlCommentPaths = new[] { typeof(Startup).GetTypeInfo().Assembly.GetName().Name ?? "RoadRegistry.Projector" }
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
                                    tags: new[] { DatabaseTag, "sql", "npgsql" });
                            }
                            else
                            {
                                health.AddSqlServer(
                                    connectionString.Value,
                                    name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                    tags: new[] { DatabaseTag, "sql", "sqlserver" });
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
                    }
                }
            })
            .AddValidatorsFromAssemblyContaining<Startup>()
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

            .AddSingleton(new IDbContextMigratorFactory[]
            {
                new ExtractsDbContextMigratorFactory()
            })
            ;

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule(new LoggingModule(_configuration, services));
        containerBuilder.RegisterModule(new ApiModule(_configuration, services));
        _applicationContainer = containerBuilder.Build();

        return new AutofacServiceProvider(_applicationContainer);
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
