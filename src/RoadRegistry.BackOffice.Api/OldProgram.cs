// namespace RoadRegistry.BackOffice.Api
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Data.SqlClient;
//     using System.IO;
//     using System.IO.Compression;
//     using System.Reflection;
//     using System.Text;
//     using System.Threading.Tasks;
//     using Amazon;
//     using Amazon.Runtime;
//     using Amazon.S3;
//     using BackOffice.Framework;
//     using BackOffice.Uploads;
//     using Be.Vlaanderen.Basisregisters.Api.Exceptions;
//     using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Logging;
//     using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
//     using Be.Vlaanderen.Basisregisters.BlobStore;
//     using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
//     using Be.Vlaanderen.Basisregisters.BlobStore.IO;
//     using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
//     using Configuration;
//     using Core;
//     using Microsoft.AspNetCore;
//     using Microsoft.AspNetCore.Hosting;
//     using Microsoft.AspNetCore.Mvc.Formatters;
//     using Microsoft.AspNetCore.ResponseCompression;
//     using Microsoft.EntityFrameworkCore;
//     using Microsoft.Extensions.Configuration;
//     using Microsoft.Extensions.DependencyInjection;
//     using Microsoft.Extensions.Logging;
//     using Microsoft.IO;
//     using Microsoft.Net.Http.Headers;
//     using Newtonsoft.Json;
//     using NodaTime;
//     using Serilog;
//     using SqlStreamStore;
//
//     public class Program
//     {
//         private const string AllowSpecificOrigin = "AllowSpecificOrigin";
//
//         public static async Task Main(string[] args)
//         {
//             AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
//                 Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);
//
//             AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
//                 Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");
//
//             var host = WebHost
//                 .CreateDefaultBuilder(args)
//                 .ConfigureAppConfiguration((hostContext, builder) =>
//                 {
//                     JsonConvert.DefaultSettings = () =>
//                         JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureDefaultForApi();
//
//                     Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//
//                     if (hostContext.HostingEnvironment.IsProduction())
//                     {
//                         builder
//                             .SetBasePath(Directory.GetCurrentDirectory());
//                     }
//
//                     builder
//                         .AddJsonFile("appsettings.json", true, false)
//                         .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
//                         .AddEnvironmentVariables()
//                         .AddCommandLine(args)
//                         .Build();
//                 })
//                 .ConfigureLogging((hostContext, builder) =>
//                 {
//                     Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
//
//                     var loggerConfiguration = new LoggerConfiguration()
//                         .ReadFrom.Configuration(hostContext.Configuration)
//                         .WriteTo.Console()
//                         .Enrich.FromLogContext()
//                         .Enrich.WithMachineName()
//                         .Enrich.WithThreadId()
//                         .Enrich.WithEnvironmentUserName();
//
//                     Log.Logger = loggerConfiguration.CreateLogger();
//
//                     builder.AddSerilog(Log.Logger);
//                 })
//                 .ConfigureServices((hostContext, builder) =>
//                 {
//                     var blobOptions = new BlobClientOptions();
//                     hostContext.Configuration.Bind(blobOptions);
//
//                     switch (blobOptions.BlobClientType)
//                     {
//                         case nameof(S3BlobClient):
//                             var s3Options = new S3BlobClientOptions();
//                             hostContext.Configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);
//
//                             // Use MINIO
//                             if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
//                             {
//                                 if (Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") == null)
//                                 {
//                                     throw new Exception("The MINIO_ACCESS_KEY environment variable was not set.");
//                                 }
//
//                                 if (Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") == null)
//                                 {
//                                     throw new Exception("The MINIO_SECRET_KEY environment variable was not set.");
//                                 }
//
//                                 builder.AddSingleton(new AmazonS3Client(
//                                         new BasicAWSCredentials(
//                                             Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY"),
//                                             Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")),
//                                         new AmazonS3Config
//                                         {
//                                             RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
//                                             ServiceURL = Environment.GetEnvironmentVariable("MINIO_SERVER"),
//                                             ForcePathStyle = true
//                                         }
//                                     )
//                                 );
//
//                             }
//                             else // Use AWS
//                             {
//                                 if (Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") == null)
//                                 {
//                                     throw new Exception("The AWS_ACCESS_KEY_ID environment variable was not set.");
//                                 }
//
//                                 if (Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") == null)
//                                 {
//                                     throw new Exception("The AWS_SECRET_ACCESS_KEY environment variable was not set.");
//                                 }
//
//                                 builder.AddSingleton(new AmazonS3Client(
//                                         new BasicAWSCredentials(
//                                             Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
//                                             Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"))
//                                     )
//                                 );
//                             }
//
//                             builder.AddSingleton<IBlobClient>(sp =>
//                                 new S3BlobClient(
//                                     sp.GetService<AmazonS3Client>(),
//                                     s3Options.BucketPrefix + WellknownBuckets.UploadsBucket
//                                 )
//                             );
//
//                             break;
//
//                         case nameof(FileBlobClient):
//                             var fileOptions = new FileBlobClientOptions();
//                             hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);
//
//                             builder.AddSingleton<IBlobClient>(sp =>
//                                 new FileBlobClient(
//                                     new DirectoryInfo(fileOptions.Directory)
//                                 )
//                             );
//                             break;
//                     }
//
//                     builder
//                         .AddAsyncInitialization()
//                         .AddAsyncInitializer<BlobStoreInitialization>()
//                         .AddAsyncInitializer<WaitForSqlStreamStore>()
//                         .AddMvcCore(options =>
//                         {
//                             options.RespectBrowserAcceptHeader = true;
//                             options.ReturnHttpNotAcceptable = true;
//
//                             options.Filters.Add(new LoggingFilterFactory());
//                             options.Filters.Add(new CorsAuthorizationFilterFactory(AllowSpecificOrigin));
//                             options.Filters.Add<OperationCancelledExceptionFilter>();
//
//                             options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
//                             options.FormatterMappings.SetMediaTypeMappingForFormat("xml",
//                                 MediaTypeHeaderValue.Parse("application/xml"));
//
//                             options.OutputFormatters.Add(new CsvOutputFormatter(new CsvFormatterOptions()));
//                             options.FormatterMappings.SetMediaTypeMappingForFormat(CsvOutputFormatter.Format,
//                                 MediaTypeHeaderValue.Parse(CsvOutputFormatter.ContentType));
//                         })
//
//                         .AddAuthorization()
//                         .AddJsonFormatters()
//                         .AddJsonOptions(options => options.SerializerSettings.ConfigureDefaultForApi())
//                         .AddCors(options => options.AddPolicy(AllowSpecificOrigin, corsPolicy => corsPolicy
//                             .AllowAnyOrigin() // TODO: Replace at a later stage with the below
//                             //.WithOrigins("http://localhost:3000", "http://localhost:5000")
//                             //apiConfiguration.CorsEnableLocalhost
//                             //    ? new[] { apiConfiguration.CorsOrigin, "http://localhost:3000", "http://localhost:5000" }
//                             //    : new[] { apiConfiguration.CorsOrigin })
//                             .WithMethods("GET", "POST", "PUT", "HEAD", "DELETE")
//                             .WithHeaders(
//                                 "accept",
//                                 "content-type",
//                                 "origin",
//                                 "x-filtering",
//                                 "x-sorting",
//                                 "x-pagination",
//                                 "authorization"
//                             )
//                             .WithExposedHeaders(
//                                 "location",
//                                 "x-filtering",
//                                 "x-sorting",
//                                 "x-pagination",
//                                 AddVersionHeaderMiddleware.HeaderName,
//                                 AddCorrelationIdToResponseMiddleware.HeaderName
//                             )
//                             .SetPreflightMaxAge(TimeSpan.FromSeconds(60 * 15))
//                             .AllowCredentials()))
//
//                         //.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
//                         //.Services
//
//                         .AddApiExplorer()
//                         .Services
//                         .AddSingleton<IStreamStore>(sp =>
//                             new MsSqlStreamStore(
//                                 new MsSqlStreamStoreSettings(sp.GetService<IConfiguration>()
//                                         .GetConnectionString(WellknownConnectionNames.Events))
//                                     {Schema = WellknownSchemas.EventSchema}))
//                         .AddSingleton<IClock>(SystemClock.Instance)
//                         .AddSingleton(new RecyclableMemoryStreamManager())
//                         .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
//                             new SqlBlobClient(
//                                 new SqlConnectionStringBuilder(sp.GetService<IConfiguration>()
//                                     .GetConnectionString(WellknownConnectionNames.Snapshots)),
//                                 WellknownSchemas.SnapshotSchema), sp.GetService<RecyclableMemoryStreamManager>()))
//                         .AddSingleton<IRoadNetworkSnapshotReader>(sp =>
//                             sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
//                         .AddSingleton<IRoadNetworkSnapshotWriter>(sp =>
//                             sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
//                         .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
//                             new CommandHandlerModule[]
//                             {
//                                 new RoadNetworkChangesArchiveCommandModule(
//                                     sp.GetService<IBlobClient>(),
//                                     sp.GetService<IStreamStore>(),
//                                     sp.GetService<IRoadNetworkSnapshotReader>(),
//                                     new ZipArchiveValidator(Encoding.UTF8),
//                                     sp.GetService<IClock>()
//                                 ),
//                                 new RoadNetworkCommandModule(
//                                     sp.GetService<IStreamStore>(),
//                                     sp.GetService<IRoadNetworkSnapshotReader>(),
//                                     sp.GetService<IClock>()
//                                 )
//                             })))
//                         .AddVersionedApiExplorer(options =>
//                         {
//                             options.GroupNameFormat = "'v'VVV";
//                             options.SubstituteApiVersionInUrl = true;
//                         })
//
//                         .AddApiVersioning(x => x.ReportApiVersions = true)
//                         .AddSwagger<Startup>(
//                             (provider, description) => new Info
//                             {
//                                 Version = description.ApiVersion.ToString(),
//                                 Title = "Wegenregister API",
//                                 Description = string.Empty,
//                                 Contact = new Contact
//                                 {
//                                     Name = "Informatie Vlaanderen",
//                                     Email = "informatie.vlaanderen@vlaanderen.be",
//                                     Url = "https://oslo.basisregisters.vlaanderen"
//                                 }
//                             },
//                             new List<string>
//                             {
//                                 typeof(Startup).GetTypeInfo().Assembly.GetName().Name,
//                             })
//
//
//                         .AddResponseCompression(options =>
//                         {
//                             options.EnableForHttps = true;
//
//                             options.Providers.Add<GzipCompressionProvider>();
//
//                             options.MimeTypes = new[]
//                             {
//                                 // General
//                                 "text/plain",
//                                 "text/csv",
//
//                                 // Static files
//                                 "text/css",
//                                 "application/javascript",
//
//                                 // MVC
//                                 "text/html",
//                                 "application/xml",
//                                 "text/xml",
//                                 "application/json",
//                                 "text/json",
//
//                                 // Fonts
//                                 "application/font-woff",
//                                 "font/otf",
//                                 "application/vnd.ms-fontobject"
//                             };
//                         })
//                         .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest)
//                         .AddDbContext<BackOfficeContext>((sp, options) => options
//                             .UseLoggerFactory(sp.GetService<ILoggerFactory>())
//                             .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
//                             .UseSqlServer(sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.BackOfficeProjections)));
//                 })
//                 .UseKestrel((hostContext, server) =>
//                 {
//                     if (hostContext.HostingEnvironment.IsDevelopment())
//                     {
//                         // ... certificate shizzle if any
//                     }
//
//                     server.AddServerHeader = false;
//                 })
//                 .CaptureStartupErrors(true)
//                 .UseContentRoot(Directory.GetCurrentDirectory())
//                 .UseWebRoot("wwwroot")
//                 .UseStartup<Startup>()
//                 .Build();
//
//             await host.InitAsync();
//             await host.RunAsync();
//         }
//     }
// }
