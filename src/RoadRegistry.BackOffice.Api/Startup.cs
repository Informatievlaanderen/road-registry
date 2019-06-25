namespace RoadRegistry.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO.Compression;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Csv;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Logging;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using BackOffice.Model;
    using BackOffice.Translation;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
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
    using NodaTime;
    using Serilog;
    using SqlStreamStore;
    using Swashbuckle.AspNetCore.Swagger;
    using RoadRegistry.BackOffice.Framework;


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
                .AddAsyncInitialization()
                .AddAsyncInitializer<BlobStoreInitialization>()
                .AddAsyncInitializer<WaitForSqlStreamStore>()
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
                .Services
                .AddSingleton<IStreamStore>(sp => new MsSqlStreamStore(new MsSqlStreamStoreSettings(sp.GetService<IConfiguration>().GetConnectionString("Events")) { Schema = "RoadRegistry" }))
                .AddSingleton<IBlobClient>(sp => new SqlBlobClient(new SqlConnectionStringBuilder(sp.GetService<IConfiguration>().GetConnectionString("Blobs")), "RoadRegistryBlobs"))
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                    new CommandHandlerModule[] {
                        new RoadNetworkChangesArchiveModule(
                            sp.GetService<IBlobClient>(),
                            sp.GetService<IStreamStore>(),
                            new ZipArchiveValidator(Encoding.UTF8),
                            new ZipArchiveTranslator(Encoding.UTF8),
                            sp.GetService<IClock>()
                        ),
                        new RoadNetworkCommandHandlerModule(
                            sp.GetService<IStreamStore>(),
                            sp.GetService<IClock>()
                        )
                    })))
                .AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                })

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
            IApiVersionDescriptionProvider provider)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
    }
}
