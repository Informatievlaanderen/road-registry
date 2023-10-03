namespace RoadRegistry.BackOffice.Api.Infrastructure;

using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using Authentication;
using Authorization;
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
using Behaviors;
using Configuration;
using Controllers.Attributes;
using Core;
using Editor.Schema;
using Extensions;
using FeatureCompare.Translators;
using FeatureToggles;
using FluentValidation;
using Framework;
using Handlers.Extensions;
using HealthChecks.UI.Client;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.HealthChecks;
using Hosts.Infrastructure.Modules;
using IdentityModel.AspNetCore.OAuth2Introspection;
using MediatR;
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
using NodaTime;
using Options;
using Product.Schema;
using Serilog.Extensions.Logging;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using Syndication.Schema;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZipArchiveWriters.Validation;
using DomainAssemblyMarker = Handlers.Sqs.DomainAssemblyMarker;
using MediatorModule = Snapshot.Handlers.MediatorModule;
using SystemClock = NodaTime.SystemClock;

public class Startup
{
    private const string DatabaseTag = "db";
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
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
                            AllowCachingResponses = false,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                            ResultStatusCodes =
                            {
                                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                            },
                        });
                    }
                }
            })
            ;
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterModule(new DataDogModule(_configuration))
            .RegisterModule<BlobClientModule>()
            .RegisterModule<MediatorModule>()
            .RegisterModule<SnapshotSqsHandlersModule>();

        builder
            .RegisterGeneric(typeof(IdentityPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder
            .RegisterModulesFromAssemblyContaining<Startup>()
            .RegisterModulesFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<Handlers.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var oAuth2IntrospectionOptions = _configuration.GetOptions<OAuth2IntrospectionOptions>(nameof(OAuth2IntrospectionOptions));
        var openIdConnectOptions = _configuration.GetOptions<OpenIdConnectOptions>();

        var baseUrl = _configuration.GetValue<string>("BaseUrl")?.TrimEnd('/') ?? string.Empty;

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
                    Headers = new[] { ApiKeyAuthenticationHandler.ApiKeyHeaderName },
                    ExposedHeaders = new[] { "Retry-After" }
                },
                Server =
                {
                    BaseUrl = baseUrl
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

                    XmlCommentPaths = new[] { typeof(Startup).GetTypeInfo().Assembly.GetName().Name },
                    MiddlewareHooks =
                    {
                        AfterSwaggerGen = options =>
                        {
                            options.AddRoadRegistrySchemaFilters();
                        }
                    }
                },
                MiddlewareHooks =
                {
                    AfterHealthChecks = builder => HealthCheckInitializer.Configure(builder, _configuration, _webHostEnvironment.IsDevelopment())
                        .AddSqlServer()
                        .AddS3(x => x
                            .CheckPermission(WellknownBuckets.UploadsBucket, Permission.Read, Permission.Write)
                            .CheckPermission(WellknownBuckets.ExtractDownloadsBucket, Permission.Read)
                            .CheckPermission(WellknownBuckets.SqsMessagesBucket, Permission.Write)
                            .CheckPermission(WellknownBuckets.SnapshotsBucket, Permission.Read)
                        )
                        .AddSqs(x => x
                            .CheckPermission(WellknownQueues.AdminQueue, Permission.Write)
                            .CheckPermission(WellknownQueues.BackOfficeQueue, Permission.Write)
                            .CheckPermission(WellknownQueues.SnapshotQueue, Permission.Write)
                        )
                        .AddLambda(x => x
                            .Check("lam-vbr-test-basisregisters-rr-sqsbackofficefunction")
                            .Check("lam-vbr-test-basisregisters-rr-sqssnapshotfunction")
                        )
                        .AddTicketing()
                        .AddAcmIdm()
                    ,
                    FluentValidation = _ =>
                    {
                        // Do not remove this handler!
                        // It must be declared to avoid FluentValidation registering all validators within current assembly.
                    },
                    Authorization = options =>
                    {
                        options
                            .AddAcmIdmAuthorization()
                            .AddAcmIdmPolicyVoInfo()
                            ;
                    }
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
            .RegisterOptions<OpenIdConnectOptions>()
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
            .AddEmailClient(_configuration)
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
                        sp.GetRequiredService<UseOvoCodeInChangeRoadNetworkFeatureToggle>(),
                        sp.GetService<IExtractUploadFailedEmailClient>(),
                        sp.GetService<ILoggerFactory>()
                    ),
                    new RoadNetworkExtractCommandModule(
                        sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                        sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                        sp.GetService<IExtractUploadFailedEmailClient>(),
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
            .AddStreetNameCache()
            .AddSingleton<TransactionZoneFeatureCompareFeatureReader>(sp => new TransactionZoneFeatureCompareFeatureReader(sp.GetRequiredService<FileEncoding>()))
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
            .Configure<ResponseOptions>(_configuration)
            .AddAcmIdmAuth(oAuth2IntrospectionOptions, openIdConnectOptions)
            .AddApiKeyAuth()
            ;

        if (_webHostEnvironment.IsDevelopment())
        {
            services.AddSingleton<IApiTokenReader>(new ConfigurationApiTokenReader(_configuration, "RoadRegistry Development ApiKey Client"));
        }
        else
        {
            services.AddSingleton<IApiTokenReader, DynamoDbApiTokenReader>();
        }

        services
            .AddMvc(options =>
            {
                options.Filters.Add<ValidationFilterAttribute>();
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter(new SerilogLoggerFactory()));
            });
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}

public class SqlServerHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, "dfd", ex);
        }
        throw new NotImplementedException();
    }
}
