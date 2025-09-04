namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Abstractions;
using Amazon;
using Amazon.DynamoDBv2;
using Asp.Versioning.ApiExplorer;
using Authentication;
using Authorization;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Handlers.Extensions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Behaviors;
using Configuration;
using Controllers;
using Controllers.Attributes;
using Core;
using Editor.Schema;
using Extensions;
using Extracten;
using FeatureCompare;
using FeatureToggles;
using FluentValidation;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Jobs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Options;
using Product.Schema;
using RoadRegistry.Extracts.Schema;
using RoadSegments;
using Serilog.Extensions.Logging;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using Sync.MunicipalityRegistry;
using SystemHealthCheck;
using SystemHealthCheck.HealthChecks;
using ZipArchiveWriters.Cleaning;
using DomainAssemblyMarker = BackOffice.Handlers.Sqs.DomainAssemblyMarker;
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
        HealthCheckService healthCheckService)
    {
        StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();

        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            serviceProvider.CreateMissingBucketsAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

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
                    EnableAuthorization = true,
                    AfterMiddleware = x =>
                    {
                        x.UseMiddleware<AddNoCacheHeadersMiddleware>();
                        x.UseHealthChecks();
                    }
                }
            })
            ;
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterModule<BlobClientModule>()
            .RegisterModule<MediatorModule>()
            .RegisterModule<SnapshotSqsHandlersModule>()
            ;

        builder
            .RegisterGeneric(typeof(IdentityPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder
            .RegisterModulesFromAssemblyContaining<Startup>()
            .RegisterModulesFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<BackOffice.Handlers.DomainAssemblyMarker>()
            .RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var oAuth2IntrospectionOptions = _configuration.GetOptions<OAuth2IntrospectionOptions>(nameof(OAuth2IntrospectionOptions));
        var openIdConnectOptions = _configuration.GetOptions<OpenIdConnectOptions>();

        var apiOptions = _configuration.GetOptions<ApiOptions>();
        apiOptions.BaseUrl = apiOptions.BaseUrl?.TrimEnd('/') ?? string.Empty;

        var featureToggles = _configuration.GetFeatureToggles<ApplicationFeatureToggle>();

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
                    Headers = [ApiKeyAuthenticationHandler.ApiKeyHeaderName],
                    ExposedHeaders = ["Retry-After"]
                },
                Server =
                {
                    BaseUrl = apiOptions.BaseUrl
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

                    XmlCommentPaths = [typeof(Startup).GetTypeInfo().Assembly.GetName().Name],
                    MiddlewareHooks =
                    {
                        AfterSwaggerGen = options =>
                        {
                            options.AddRoadRegistrySchemaFilters();
                            options.CustomSchemaIds(t => SwashbuckleHelpers.GetCustomSchemaId(t)
                                                         ?? SwashbuckleHelpers.PublicApiDefaultSchemaIdSelector(t));
                        }
                    }
                },
                MiddlewareHooks =
                {
                    FluentValidation = _ =>
                    {
                        // Do not remove this handler!
                        // It must be declared to avoid FluentValidation registering all validators within current assembly.
                    },
                    Authorization = options =>
                    {
                        var blacklistedOvoCodes =  _configuration
                            .GetSection("BlacklistedOvoCodes")
                            .GetChildren()
                            .Select(c => c.Value!)
                            .ToArray();

                        options
                            .AddRoadPolicies(blacklistedOvoCodes)
                            .AddAcmIdmPolicyVoInfo()
                            ;
                    }
                }
            })
            .AddAcmIdmAuthorizationHandlers()
            .AddSingleton(_ => new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            .AddSingleton(FileEncoding.WindowsAnsi)
            .AddStreetNameCache()
            .AddFeatureCompare()
            .AddSingleton<IBeforeFeatureCompareZipArchiveCleanerFactory, BeforeFeatureCompareZipArchiveCleanerFactory>()
            .AddSingleton<ProblemDetailsHelper>()
            .RegisterOptions<ZipArchiveWriterOptions>()
            .RegisterOptions<ExtractDownloadsOptions>()
            .RegisterOptions<ExtractUploadsOptions>()
            .RegisterOptions<OpenIdConnectOptions>()
            .AddStreamStore()
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddEventEnricher()
            .AddSingleton(new WKTReader(
                new NtsGeometryServices(
                    GeometryConfiguration.GeometryFactory.PrecisionModel,
                    GeometryConfiguration.GeometryFactory.SRID
                )
            ))
            .AddSingleton(new RecyclableMemoryStreamManager())
            .AddRoadRegistrySnapshot()
            .AddRoadNetworkEventWriter()
            .AddScoped(_ => new EventSourcedEntityMap())
            .AddEmailClient()
            .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
            [
                new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                        sp.GetService<ITransactionZoneZipArchiveReader>(),
                        sp.GetService<IClock>(),
                        sp.GetService<ILoggerFactory>()
                    ),
                    CommandModules.RoadNetwork(sp),
                    new RoadNetworkExtractCommandModule(
                        sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<ILifetimeScope>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        sp.GetService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                        sp.GetService<IExtractUploadFailedEmailClient>(),
                        sp.GetService<IClock>(),
                        sp.GetService<ILoggerFactory>()
                    )
            ])))
            .AddDbContextFactory<EditorContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.EditorProjections),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            )
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.ProductProjections)
                ))
            .AddDbContext<MunicipalityEventConsumerContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.MunicipalityEventConsumer),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            )
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            .AddOrganizationCache()
            .AddScoped<IRoadSegmentRepository, RoadSegmentRepository>()
            .AddValidatorsFromAssemblyContaining<Startup>()
            .AddValidatorsFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<BackOffice.Handlers.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>()
            .AddFeatureToggles(featureToggles)
            .AddTicketing()
            .AddRoadRegistrySnapshot()
            .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
            .AddHealthCommandQueue()
            .AddRoadNetworkCommandQueue()
            .AddOrganizationCommandQueue()
            .AddSingleton(apiOptions)
            .Configure<ResponseOptions>(_configuration)
            .AddScoped<BackofficeApiControllerContext>()
            .AddSingleton<IExtractShapefileContourReader, ExtractShapefileContourReader>()

            .AddSystemHealthChecks([
                typeof(CommandHostSystemHealthCheck),
                typeof(EventHostSystemHealthCheck),
                typeof(ExtractHostSystemHealthCheck),
                typeof(BackOfficeLambdaSystemHealthCheck),
                typeof(SnapshotLambdaSystemHealthCheck),
            ])

            // Jobs
            .Configure<JobsBucketOptions>(_configuration.GetSection(JobsBucketOptions.ConfigKey))
            .AddJobsContext()
            .AddSingleton<IPagedUriGenerator, PagedUriGenerator>()
            .AddScoped<AmazonS3DownloadFileUrlPresigner>()
            .AddScoped<IDownloadFileUrlPresigner>(sp =>
            {
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                if (hostEnvironment.IsDevelopment())
                {
                    return new AnonymousBackOfficeApiDownloadFileUrlPresigner(sp.GetRequiredService<ApiOptions>());
                }

                return sp.GetRequiredService<AmazonS3DownloadFileUrlPresigner>();
            })
            .AddScoped<IJobUploadUrlPresigner>(sp =>
            {
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                if (hostEnvironment.IsDevelopment())
                {
                    return new AnonymousBackOfficeApiJobUploadUrlPresigner(sp.GetRequiredService<ApiOptions>());
                }

                return new AmazonS3JobUploadUrlPresigner(sp.GetRequiredService<IAmazonS3Extended>(), sp.GetRequiredService<IOptions<JobsBucketOptions>>());
            })
            .AddDistributedMemoryCache()
            .AddAcmIdmAuthentication(oAuth2IntrospectionOptions, openIdConnectOptions)
            .AddApiKeyAuth()
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
