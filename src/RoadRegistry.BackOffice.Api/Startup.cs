namespace RoadRegistry.BackOffice.Api;

using Amazon;
using Amazon.DynamoDBv2;
using Autofac;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Configuration;
using Extensions;
using FluentValidation;
using Handlers.Extensions;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Configuration;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Product.Schema;
using RoadRegistry.Syndication.Schema;
using SqlStreamStore;
using System;
using System.Linq;
using System.Text;

public class Startup
{
    private const string DatabaseTag = "db";
    private readonly IConfiguration _configuration;

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
        ApiDataDogToggle datadogToggle,
        ApiDebugDataDogToggle debugDataDogToggle,
        HealthCheckService healthCheckService)
    {
        StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();

        app
            .UseDataDog<Startup>(new DataDogOptions
            {
                Common =
                {
                    ServiceProvider = serviceProvider,
                    LoggerFactory = loggerFactory
                },
                Toggles =
                {
                    Enable = datadogToggle,
                    Debug = debugDataDogToggle
                },
                Tracing =
                {
                    ServiceName = _configuration["DataDog:ServiceName"]
                }
            })
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
                    Info = groupName => $"Basisregisters Vlaanderen - Road Registry API {groupName}",
                    CSharpClientOptions =
                    {
                        ClassName = "RoadRegistry",
                        Namespace = "Be.Vlaanderen.Basisregisters"
                    },
                    TypeScriptClientOptions =
                    {
                        ClassName = "RoadRegistry"
                    }
                },
                Server =
                {
                    PoweredByName = "Vlaamse overheid - Basisregisters Vlaanderen",
                    ServerName = "Digitaal Vlaanderen"
                },
                MiddlewareHooks =
                {
                    AfterMiddleware = x =>
                    {
                        x.UseMiddleware<AddNoCacheHeadersMiddleware>();
                        x.UseHealthChecks(new PathString("/health"), Program.HostingPort, new HealthCheckOptions
                        {
                            ResultStatusCodes =
                            {
                                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                            }
                        });
                    }
                }
            });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .ConfigureDefaultForApi<Startup>(new StartupConfigureOptions
            {
                Cors =
                {
                    Origins = _configuration
                        .GetSection("Cors")
                        .GetChildren()
                        .Select(c => c.Value)
                        .ToArray(),
                    ExposedHeaders = new[] { "Retry-After" }
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
                    }
                },
                MiddlewareHooks =
                {
                    AfterHealthChecks = health =>
                    {
                        var connectionStrings = _configuration
                            .GetSection("ConnectionStrings")
                            .GetChildren();

                        foreach (var connectionString in connectionStrings)
                            health.AddSqlServer(
                                connectionString.Value,
                                name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });
                    },
                    FluentValidation = _ =>
                    {
                        // Do not remove this handler!
                        // It must be declared to avoid FluentValidation registering all validators within current assembly.
                    }
                }
            })
            .AddSingleton(new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            .AddSingleton<IZipArchiveBeforeFeatureCompareValidator>(new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
            .AddSingleton<IZipArchiveAfterFeatureCompareValidator>(new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
            .AddSingleton<ProblemDetailsHelper>()
            .RegisterOptions<ZipArchiveWriterOptions>()
            .RegisterOptions<ExtractDownloadsOptions>()
            .RegisterOptions<ExtractUploadsOptions>()
            .RegisterOptions<FeatureCompareMessagingOptions>()
            .AddStreamStore()
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(new WKTReader(
                new NtsGeometryServices(
                    GeometryConfiguration.GeometryFactory.PrecisionModel,
                    GeometryConfiguration.GeometryFactory.SRID
                )
            ))
            .AddSingleton(new RecyclableMemoryStreamManager())
            .AddSingleton<IBlobClient>(new SqlBlobClient(
                new SqlConnectionStringBuilder(
                    _configuration.GetConnectionString(WellknownConnectionNames.Snapshots)),
                WellknownSchemas.SnapshotSchema))
            .AddRoadRegistrySnapshot()
            .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
            .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                            new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkUploadsBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILoggerFactory>()
                            ),
                            new RoadNetworkCommandModule(
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILoggerFactory>()
                            ),
                            new RoadNetworkExtractCommandModule(
                                sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILoggerFactory>()
                            )
                })))
            .AddScoped(sp => new TraceDbConnection<EditorContext>(
                new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.EditorProjections)),
                sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
            .AddScoped(sp => new TraceDbConnection<SyndicationContext>(
                new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections)),
                sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddSingleton<Func<SyndicationContext>>(sp =>
                () =>
                    new SyndicationContext(
                        new DbContextOptionsBuilder<SyndicationContext>()
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseSqlServer(
                                _configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                options => options
                                    .EnableRetryOnFailure()
                            )
                            .Options)
            )
            .AddDbContext<EditorContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<TraceDbConnection<EditorContext>>(),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            )
            .AddScoped(sp => new TraceDbConnection<ProductContext>(
                new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ProductProjections)),
                sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<TraceDbConnection<ProductContext>>()))
            .AddValidatorsAsScopedFromAssemblyContaining<Startup>()
            .AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<Handlers.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<Handlers.Sqs.DomainAssemblyMarker>()
            .AddFeatureToggles<ApplicationFeatureToggle>(_configuration)
            .AddTicketing()
            .AddRoadRegistrySnapshot()
            ;
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new DataDogModule(_configuration));
        builder.RegisterModule<BlobClientModule>();
        builder.RegisterModulesFromAssemblyContaining<Startup>();
        builder.RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
        builder.RegisterModulesFromAssemblyContaining<Handlers.DomainAssemblyMarker>();
        builder.RegisterModulesFromAssemblyContaining<Handlers.Sqs.DomainAssemblyMarker>();
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
