namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Configuration;
using Controllers.Attributes;
using Core;
using Editor.Schema;
using FluentValidation;
using Framework;
using Handlers.Extensions;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
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
using NisCodeService.Abstractions;
using NisCodeService.Proxy.HttpProxy;
using NodaTime;
using Product.Schema;
using Serilog.Extensions.Logging;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using Syndication.Schema;
using ZipArchiveWriters.Validation;
using DomainAssemblyMarker = Handlers.Sqs.DomainAssemblyMarker;
using MediatorModule = Snapshot.Handlers.MediatorModule;
using SystemClock = NodaTime.SystemClock;

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

        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            serviceProvider.CreateMissingBucketsAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            serviceProvider.CreateMissingQueuesAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

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
                    Enable = new Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft.ApiDataDogToggle(datadogToggle.FeatureEnabled),
                    Debug = new Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft.ApiDebugDataDogToggle(debugDataDogToggle.FeatureEnabled)
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

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterModule(new DataDogModule(_configuration))
            .RegisterModule<BlobClientModule>()
            .RegisterModule<MediatorModule>()
            .RegisterModule<SnapshotSqsHandlersModule>()
            ;

        builder
            .RegisterModulesFromAssemblyContaining<Startup>()
            .RegisterModulesFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<Handlers.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var oAuth2IntrospectionOptions = _configuration
            .GetSection(nameof(OAuth2IntrospectionOptions))
            .Get<OAuth2IntrospectionOptions>();

        var baseUrl = _configuration.GetValue<string>("BaseUrl");
        var baseUrlForExceptions = baseUrl.EndsWith("/")
            ? baseUrl.Substring(0, baseUrl.Length - 1)
            : baseUrl;
        
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
                    Headers = new[] {ApiKeyAuthAttribute.ApiKeyHeaderName},
                    ExposedHeaders = new[] { "Retry-After" }
                },
                Server =
                {
                    BaseUrl = baseUrlForExceptions
                },
                Swagger =
                {
                    ApiInfo = (_, description) => new OpenApiInfo
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

                    XmlCommentPaths = new[] { typeof(Startup).GetTypeInfo().Assembly.GetName().Name }
                },
                MiddlewareHooks =
                {
                    AfterHealthChecks = health =>
                    {
                        var connectionStrings = _configuration
                            .GetSection("ConnectionStrings")
                            .GetChildren();

                        foreach (var connectionString in connectionStrings)
                        {
                            health.AddSqlServer(
                                connectionString.Value,
                                name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });
                        }
                    },
                    FluentValidation = _ =>
                    {
                        // Do not remove this handler!
                        // It must be declared to avoid FluentValidation registering all validators within current assembly.
                    },
                    Authorization = options => { options.AddAcmIdmAuthorization(); }
                }
            })
            .AddAcmIdmAuthorizationHandlers()
            .AddSingleton(new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            .AddSingleton(FileEncoding.WindowsAnsi)
            .AddSingleton<IZipArchiveBeforeFeatureCompareValidator, ZipArchiveBeforeFeatureCompareValidator>()
            .AddSingleton<IZipArchiveAfterFeatureCompareValidator, ZipArchiveAfterFeatureCompareValidator>()
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
            .AddRoadNetworkEventWriter()
            .AddScoped(_ => new EventSourcedEntityMap())
            .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                        sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                        sp.GetService<IClock>(),
                        sp.GetService<ILoggerFactory>()
                    ),
                    new RoadNetworkCommandModule(
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IClock>(),
                        sp.GetService<ILoggerFactory>()
                    ),
                    new RoadNetworkExtractCommandModule(
                        sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
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
            .AddValidatorsFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<Handlers.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>()
            .AddFeatureToggles<ApplicationFeatureToggle>(_configuration)
            .AddTicketing()
            .AddRoadRegistrySnapshot()
            .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
            .AddRoadNetworkCommandQueue()
            .AddRoadNetworkSnapshotStrategyOptions()
            .AddAcmIdmAuth(oAuth2IntrospectionOptions!);

        services
            .AddMvc(options =>
            {
                options.Filters.Add<ValidationFilterAttribute>();
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter(new SerilogLoggerFactory()));
            });

        //JsonConvert.DefaultSettings = () =>
        //{
        //    var settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        //    foreach (var converter in WellKnownJsonConverters.Converters)
        //    {
        //        settings.Converters.Add(converter);
        //    }

        //    return settings;
        //};
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}

public static class ServiceCollectionExtensions
{
    public static AuthenticationBuilder AddAcmIdmAuth(
        this IServiceCollection services,
        OAuth2IntrospectionOptions oAuth2IntrospectionOptions)
    {
        return services
            .AddHttpProxyNisCodeService()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddOAuth2Introspection(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = oAuth2IntrospectionOptions.ClientId;
                options.ClientSecret = oAuth2IntrospectionOptions.ClientSecret;
                options.Authority = oAuth2IntrospectionOptions.Authority;
                options.IntrospectionEndpoint = oAuth2IntrospectionOptions.IntrospectionEndpoint;
            });
    }

    public static IServiceCollection AddHttpProxyNisCodeService(this IServiceCollection services)
    {
        services
            .AddHttpClient<INisCodeService, HttpProxyNisCodeService>((sp, c) =>
            {
                var nisCodeServiceUrl = sp.GetRequiredService<IConfiguration>().GetValue<string>("NisCodeServiceUrl");
                if (string.IsNullOrWhiteSpace(nisCodeServiceUrl))
                {
                    throw new ConfigurationErrorsException("Configuration should have a value for \"NisCodeServiceUrl\".");
                }

                c.BaseAddress = new Uri(nisCodeServiceUrl.TrimEnd('/'));
            });
        return services;
    }
}
