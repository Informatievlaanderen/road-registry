namespace Wegenregister.Api.Oslo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.AspNetCore.Mvc.Formatters.Csv;
    using Aiv.Vbr.AspNetCore.Mvc.Formatters.Json;
    using Aiv.Vbr.AspNetCore.Mvc.Logging;
    using Aiv.Vbr.AspNetCore.Mvc.Middleware;
    using Aiv.Vbr.CommandHandling.Idempotency;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Cors.Internal;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using Modules;
    using Serilog;
    using SqlStreamStore;
    using Swagger;

    /// <summary>Represents the startup process for the application.</summary>
    public class Startup
    {
        public const string AllowSpecificOrigin = "AllowSpecificOrigin";

        private IContainer _applicationContainer;

        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        /// <summary>Configures services for the application.</summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;

                    options.Filters.Add(new LoggingFilterFactory());
                    options.Filters.Add(new CorsAuthorizationFilterFactory(AllowSpecificOrigin));
                    options.Filters.Add<OperationCancelledExceptionFilter>();

                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    options.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeHeaderValue.Parse("application/xml"));

                    options.OutputFormatters.Add(new CsvOutputFormatter(new CsvFormatterOptions()));
                    options.FormatterMappings.SetMediaTypeMappingForFormat(CsvOutputFormatter.Format, MediaTypeHeaderValue.Parse(CsvOutputFormatter.ContentType));
                })

                .AddAuthorization()

                .AddJsonFormatters()
                .AddJsonOptions(options => options.SerializerSettings.ConfigureDefaultForApi())

                .AddCors(options => options.AddPolicy(AllowSpecificOrigin, corsPolicy => corsPolicy
                    .AllowAnyOrigin() // TODO: Replace at a later stage with the below
                    //.WithOrigins("http://localhost:3000", "http://localhost:5000")
                        //apiConfiguration.CorsEnableLocalhost
                        //    ? new[] { apiConfiguration.CorsOrigin, "http://localhost:3000", "http://localhost:5000" }
                        //    : new[] { apiConfiguration.CorsOrigin })
                    .WithMethods("GET", "POST", "PUT", "HEAD", "DELETE")
                    .WithHeaders("accept", "content-type", "origin", "x-filtering", "x-sorting", "x-pagination", "authorization")
                    .WithExposedHeaders("location", "x-filtering", "x-sorting", "x-pagination", AddVersionHeaderMiddleware.HeaderName, AddCorrelationIdToResponseMiddleware.HeaderName)
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(60 * 15))
                    .AllowCredentials()))

                //.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                //.Services

                .AddApiExplorer()
                .AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV")
                .Services

                // TODO: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/370 Integrate better with Microsoft.AspNetCore.Mvc.Versioning
                // https://github.com/xperiandri/FwDay2017/blob/master/Demo3End/Startup.cs
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/244#issuecomment-292405937
                // TODO: Configure more, https://github.com/Microsoft/aspnet-api-versioning/wiki/API-Versioning-Options
                .AddApiVersioning(x => x.ReportApiVersions = true)
                .AddSwagger()

                .AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;

                    options.Providers.Add<GzipCompressionProvider>();

                    options.MimeTypes = new[]
                    {
                        // General
                        "text/plain",
                        "text/csv",

                        // Static files
                        "text/css",
                        "application/javascript",

                        // MVC
                        "text/html",
                        "application/xml",
                        "text/xml",
                        "application/json",
                        "text/json",

                        // Fonts
                        "application/font-woff",
                        "font/otf",
                        "application/vnd.ms-fontobject"
                    };
                })
                .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new ApiModule(_configuration, services, _loggerFactory));
            _applicationContainer = containerBuilder.Build();

            return new AutofacServiceProvider(_applicationContainer);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime appLifetime,
            ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider provider,
            EnvelopeFactory envelopeFactory,
            IStreamStore streamStore)
        {
            EnsureSqlStreamStoreSchema(streamStore);

            if (env.IsDevelopment())
                app
                    .UseDeveloperExceptionPage()
                    .UseDatabaseErrorPage()
                    .UseBrowserLink();

            app
                .UseCors(policyName: AllowSpecificOrigin)

                .UseApiExceptionHandler(loggerFactory, AllowSpecificOrigin)

                .UseIdempotencyDatabaseMigrations()

                .UseMiddleware<EnableRequestRewindMiddleware>()
                .UseMiddleware<AddCorrelationIdToLogContextMiddleware>()
                .UseMiddleware<AddCorrelationIdToResponseMiddleware>()
                .UseMiddleware<AddCorrelationIdMiddleware>()
                .UseMiddleware<AddHttpSecurityHeadersMiddleware>()
                .UseMiddleware<AddRemoteIpAddressMiddleware>()
                .UseMiddleware<AddVersionHeaderMiddleware>()
                .UseMiddleware<AddNoCacheHeadersMiddleware>()

                .UseMiddleware<DefaultResponseCompressionQualityMiddleware>(new Dictionary<string, double>
                {
                    { "br", 1.0 },
                    { "gzip", 0.9 }
                })
                .UseResponseCompression()

                .UseDefaultFiles()
                .UseStaticFiles()

                .UseMvc()

                .UseSwagger(x => x.RouteTemplate = "docs/{documentName}/docs.json")
                .UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "docs";

                    provider.ApiVersionDescriptions.ToList()
                        .ForEach(description => x.SwaggerEndpoint($"/docs/{description.GroupName}/docs.json",
                            $"Vlaamse basisregisters - Wegenregister API {description.GroupName}"));
                });

            RegisterApplicationLifetimeHandling(appLifetime);
        }

        private void RegisterApplicationLifetimeHandling(IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(() => Log.Information("Application started."));

            appLifetime.ApplicationStopping.Register(() =>
            {
                Log.Information("Application stopping.");
                Log.CloseAndFlush();
            });

            appLifetime.ApplicationStopped.Register(() => _applicationContainer.Dispose());

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                appLifetime.StopApplication();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private static void EnsureSqlStreamStoreSchema(IStreamStore streamStore)
        {
            if (!(streamStore is MsSqlStreamStore msSqlStreamStore))
                return;

            var checkSchemaResult = msSqlStreamStore.CheckSchema().GetAwaiter().GetResult();
            if (!checkSchemaResult.IsMatch())
                msSqlStreamStore.CreateSchema().GetAwaiter().GetResult();
        }
    }
}
