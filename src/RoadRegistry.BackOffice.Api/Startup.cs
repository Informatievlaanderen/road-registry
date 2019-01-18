namespace RoadRegistry.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO.Compression;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.AspNetCore.Mvc.Formatters.Csv;
    using Aiv.Vbr.AspNetCore.Mvc.Formatters.Json;
    using Aiv.Vbr.AspNetCore.Mvc.Logging;
    using Aiv.Vbr.AspNetCore.Mvc.Middleware;
    using Aiv.Vbr.AspNetCore.Swagger;
    using Aiv.Vbr.AspNetCore.Swagger.ReDoc;
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
    using Swashbuckle.AspNetCore.Swagger;


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
                    .WithHeaders(
                        "accept",
                        "content-type",
                        "origin",
                        "x-filtering",
                        "x-sorting",
                        "x-pagination",
                        "authorization"
                    )
                    .WithExposedHeaders(
                        "location",
                        "x-filtering",
                        "x-sorting",
                        "x-pagination",
                        AddVersionHeaderMiddleware.HeaderName,
                        AddCorrelationIdToResponseMiddleware.HeaderName
                    )
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(60 * 15))
                    .AllowCredentials()))

                //.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                //.Services

                .AddApiExplorer()
                .AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                })
                .Services

                .AddApiVersioning(x => x.ReportApiVersions = true)
                .AddSwagger<Startup>(
                    (provider, description) => new Info
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = "Wegenregister API",
                        Description = string.Empty,
                        Contact = new Contact
                        {
                            Name = "Informatie Vlaanderen",
                            Email = "informatie.vlaanderen@vlaanderen.be",
                            Url = "https://oslo.basisregisters.vlaanderen"
                        }
                    },
                    new List<string>
                    {
                        typeof(Startup).GetTypeInfo().Assembly.GetName().Name,
                    })


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
            IConfiguration config)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var connectionStringBuilder = new SqlConnectionStringBuilder(config.GetConnectionString("Events"));
            WaitForStreamStore(connectionStringBuilder);

            //EnsureSqlStreamStoreSchema(streamStore);

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

                .UseSwaggerDocumentation(provider, groupName => $"Basisregisters.Vlaanderen - Wegenregister API {groupName}");

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

        private static void WaitForStreamStore(SqlConnectionStringBuilder builder)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(builder.ConnectionString)
                    {
                        Schema = "RoadRegistry"
                    }))
                    {
                        streamStore.ReadHeadPosition(CancellationToken.None).GetAwaiter().GetResult();
                        exit = true;
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
