namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extracts;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

// Guards that Swashbuckle can build the OpenAPI document for every API version. If a controller/DTO introduces a
// schema conflict (e.g. a duplicate CustomSwaggerSchemaId or an unresolvable type) this fails here instead of only at
// runtime as a 500 on /docs. It composes the app's DI (ConfigureServices + the Autofac ConfigureContainer) but does
// not run Startup.Configure, so no database/infrastructure is required.
//
// Runs in the non-parallel "Swagger" collection: composing the API DI briefly mutates FluentValidation's global static
// resolvers, which would otherwise race with the parallel validator tests in this assembly.
[Collection("Swagger")]
public class SwaggerTests
{
    [Fact]
    public void SwaggerDocumentCanBeBuiltForEveryApiVersion()
    {
        var serviceProvider = BuildApiServiceProvider();

        // Some response-example providers read IHttpContextAccessor.HttpContext (to build a problem-details instance URI).
        // HttpContextAccessor stores it in a static AsyncLocal, so a context set here flows into GetSwagger below.
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        new HttpContextAccessor().HttpContext = httpContext;

        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var swaggerGeneratorOptions = serviceProvider.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value;

        // Drop the framework operation filters that rely on the running web-host auth runtime (e.g. the
        // AuthorizationResponse filter): they add response codes based on policies and would NRE outside a real host.
        // They are orthogonal to schema generation, which is what makes /docs 500 and is what this test guards.
        foreach (var operationFilter in swaggerGeneratorOptions.OperationFilters
                     .Where(f => f.GetType().Namespace?.StartsWith("Be.Vlaanderen.Basisregisters.AspNetCore.Swagger") == true)
                     .ToList())
        {
            swaggerGeneratorOptions.OperationFilters.Remove(operationFilter);
        }

        // The registered OpenAPI documents (e.g. "v1", "v2") - exactly the documents /docs asks Swashbuckle to build.
        var documentNames = swaggerGeneratorOptions.SwaggerDocs.Keys;

        documentNames.Should().NotBeEmpty();

        foreach (var documentName in documentNames)
        {
            var buildSwaggerDocument = () => swaggerProvider.GetSwagger(documentName);

            buildSwaggerDocument.Should().NotThrow(
                $"the OpenAPI document for '{documentName}' must be buildable so /docs does not return a 500");
        }
    }

    [Fact]
    public void ChangeAttributesV2_SchemasAreRequiredAndOrderedCorrectly()
    {
        var serviceProvider = BuildApiServiceProvider();
        var schemaGenerator = serviceProvider.GetRequiredService<ISchemaGenerator>();

        var schemaRepository = new SchemaRepository();
        schemaGenerator.GenerateSchema(typeof(StraatnaamParameters), schemaRepository);
        schemaGenerator.GenerateSchema(typeof(MorfologieParameters), schemaRepository);
        schemaGenerator.GenerateSchema(typeof(ChangeRoadSegmentAttributeV2Parameters), schemaRepository);

        // The list of road segments to change is required.
        schemaRepository.Schemas[nameof(ChangeRoadSegmentAttributeV2Parameters)].Required
            .Should().Contain("wegsegmenten");

        // A sided VanTot derivative orders kant, vanPositie, totPositie and then the attribute; only the attribute
        // itself is required (not kant / vanPositie / totPositie).
        var straatnaam = schemaRepository.Schemas[nameof(StraatnaamParameters)];
        straatnaam.Properties.Keys.Should().Equal("kant", "vanPositie", "totPositie", "identificator");
        straatnaam.Required.Should().BeEquivalentTo(new[] { "identificator" });

        // A non-sided VanTot derivative orders vanPositie, totPositie and then the attribute; only the attribute is required.
        var morfologie = schemaRepository.Schemas[nameof(MorfologieParameters)];
        morfologie.Properties.Keys.Should().Equal("vanPositie", "totPositie", "morfologie");
        morfologie.Required.Should().BeEquivalentTo(new[] { "morfologie" });
    }

    [Fact]
    public void ChangeAttributesV2_HasARequestExample()
    {
        var serviceProvider = BuildApiServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        new HttpContextAccessor().HttpContext = httpContext;

        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var swaggerGeneratorOptions = serviceProvider.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value;

        foreach (var operationFilter in swaggerGeneratorOptions.OperationFilters
                     .Where(f => f.GetType().Namespace?.StartsWith("Be.Vlaanderen.Basisregisters.AspNetCore.Swagger") == true)
                     .ToList())
        {
            swaggerGeneratorOptions.OperationFilters.Remove(operationFilter);
        }

        // The operation shows up in every document it is part of; each of them must carry the example.
        var operations = swaggerGeneratorOptions.SwaggerDocs.Keys
            .Select(documentName => swaggerProvider.GetSwagger(documentName))
            .SelectMany(document => document.Paths.Values)
            .SelectMany(path => path.Operations!.Values)
            .Where(x => x.OperationId == nameof(RoadSegmentsController.ChangeRoadSegmentAttributesV2))
            .ToList();

        operations.Should().NotBeEmpty();
        operations.SelectMany(x => x.RequestBody!.Content!.Values)
            .Should().NotBeEmpty()
            .And.OnlyContain(x => x.Example != null,
                "the endpoint declares a SwaggerRequestExample which must end up in the OpenAPI document");
    }

    private static IServiceProvider BuildApiServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var startup = new Startup(configuration);

        var apiAssembly = typeof(Startup).Assembly;

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        // The API pipeline (Swagger options) needs a hosting environment; provide a minimal one for the test.
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(apiAssembly.GetName().Name!));

        // ConfigureDefaultForApi mutates FluentValidation's global static resolvers. Snapshot and restore them so this
        // test doesn't pollute the other (validator) tests running in the same process.
        var savedDisplayNameResolver = ValidatorOptions.Global.DisplayNameResolver;
        var savedPropertyNameResolver = ValidatorOptions.Global.PropertyNameResolver;
        try
        {
            startup.ConfigureServices(services);
        }
        finally
        {
            ValidatorOptions.Global.DisplayNameResolver = savedDisplayNameResolver;
            ValidatorOptions.Global.PropertyNameResolver = savedPropertyNameResolver;
        }

        // For unit testing use the UTF8 file encoding rather than the Windows-1252 (code page 1252) one, which would
        // otherwise require the code-pages encoding provider that only the real host registers.
        services.AddSingleton(FileEncoding.UTF8);

        // ApiExplorer discovers controllers from the registered application parts. In the real host the API assembly is
        // the entry assembly; here we add it explicitly so the real controllers (and thus their schemas) are included.
        var mvcBuilder = services.AddControllers();
        if (mvcBuilder.PartManager.ApplicationParts.OfType<AssemblyPart>().All(p => p.Assembly != apiAssembly))
        {
            mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(apiAssembly));
        }

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        startup.ConfigureContainer(containerBuilder);

        return new AutofacServiceProvider(containerBuilder.Build());
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string applicationName)
        {
            ApplicationName = applicationName;
            ContentRootPath = AppContext.BaseDirectory;
            WebRootPath = AppContext.BaseDirectory;
            ContentRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(AppContext.BaseDirectory);
            WebRootFileProvider = ContentRootFileProvider;
        }

        public string ApplicationName { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public string WebRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
    }
}
