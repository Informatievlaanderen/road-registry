namespace RoadRegistry.BackOffice.Api;

using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Configuration;
using Extensions;
using FluentValidation;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
                    ApplicationContainer = _applicationContainer,
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

    public IServiceProvider ConfigureServices(IServiceCollection services)
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
                    }
                }
            })
            .AddValidatorsFromAssemblyContaining<Startup>()
            .AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<Handlers.DomainAssemblyMarker>()
            .AddValidatorsFromAssemblyContaining<Handlers.Sqs.DomainAssemblyMarker>()
            .AddFeatureToggles<ApplicationFeatureToggle>(_configuration)
            .AddTicketing();

        var builder = new ContainerBuilder();
        builder.RegisterModule(new DataDogModule(_configuration));
        
        builder.RegisterModulesFromAssemblyContaining<Startup>();
        builder.RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
        builder.RegisterModulesFromAssemblyContaining<Handlers.DomainAssemblyMarker>();
        builder.RegisterModulesFromAssemblyContaining<Handlers.Sqs.DomainAssemblyMarker>();

        builder.Populate(services);

        _applicationContainer = builder.Build();

        return new AutofacServiceProvider(_applicationContainer);
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
    {
        return $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Road Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
