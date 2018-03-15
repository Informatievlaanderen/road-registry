namespace Wegenregister.Api.Oslo.Infrastructure.Swagger
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Aiv.Vbr.AspNetCore.Swagger;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.Examples;
    using Swashbuckle.AspNetCore.Swagger;

    /// <summary>
    /// Configure Swagger schema generation.
    /// </summary>
    public static class Swagger
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(x =>
            {
                x.DescribeAllEnumsAsStrings();
                x.DescribeStringEnumsInCamelCase();
                x.DescribeAllParametersInCamelCase();

                //x.AddSecurityDefinition("bl", new ApiKeyScheme(){});

                x.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).GetTypeInfo().Assembly.GetName().Name}.xml"));
                x.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Aiv.Vbr.Configuration.Api.xml"));

                x.DocInclusionPredicate((version, description) =>
                {
                    //  bit of a hack, keep an eye on future releases of swagger
                    var values = description.RelativePath
                        .Split('/')
                        .Skip(1);

                    description.RelativePath = version + "/" + string.Join("/", values);

                    var versionParameter = description.ParameterDescriptions
                        .SingleOrDefault(p => p.Name == "version");

                    if (versionParameter != null)
                        description.ParameterDescriptions.Remove(versionParameter);

                    return true;
                });

                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                    x.SwaggerDoc(description.GroupName, new Info
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = "Wegenregister API",
                        Description = GetApiLeadingText(provider, description),
                        Contact = new Contact
                        {
                            Name = "Informatie Vlaanderen",
                            Email = "wegenregister@vlaanderen.be",
                            Url = "https://weg.basisregisters.api.vlaanderen.be"
                        }
                    });

                x.SchemaFilter<AutoRestSchemaFilter>();

                x.OperationFilter<SwaggerDefaultValues>();
                x.OperationFilter<TagByApiExplorerSettingsOperationFilter>();
                x.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                x.OperationFilter<AuthorizationResponseOperationFilter>();
                x.OperationFilter<ExamplesOperationFilter>(); // [SwaggerRequestExample] & [SwaggerResponseExample]
                x.OperationFilter<DescriptionOperationFilter>(); // [Description] on Response properties
            });

            return services;
        }

        private static string GetApiLeadingText(IApiVersionDescriptionProvider provider, ApiVersionDescription description)
        {
            var text = new StringBuilder(1000);

            text.Append(
$@"# Introductie

Welkom bij de referentie voor de REST API van het Wegenregister!

[REST](http://en.wikipedia.org/wiki/REST_API) is een webserviceprotocol dat zich leent voor snelle ontwikkeling door het gebruik van HTTP- en JSON-technologie.

Het Wegenregister stelt u in staat om:
* Alles te weten te komen rond de Belgische wegen.

## Contact

U kan ons bereiken via [wegenregister@vlaanderen.be](mailto:wegenregister@vlaanderen.be).

## Ontsluitingen

De REST API van het Wegenregister is te bereiken via volgende ontsluitingen.

Doelpubliek | Basis URL voor de REST ontsluitingen                              |
----------- | ----------------------------------------------------------------- |
Iedereen    | https://weg.basisregisters.vlaanderen/{description.GroupName} |

## Toegang tot de API

U kan anoniem gebruik maken van de API, echter is deze beperkt in het aantal verzoeken dat u tegelijk kan sturen.

Wenst u volwaardige toegang tot de api, dan kan u zich [hier aanmelden als ontwikkelaar](https://apigeeportaal).

# Authenticatie

# Verzoeken en Responsen

## Foutmeldingen

Onze API gebruikt [Problem Details for HTTP APIs (RFC7807)](https://tools.ietf.org/html/rfc7807) om foutmeldingen te ontsluiten. Een foutmelding zal resulteren in volgende datastructuur:

```
{{
  ""type"": ""string"",
  ""title"": ""string"",
  ""detail"": ""string"",
  ""status"": number,
  ""instance"": ""string""
}}
```

# Paginering

# Versionering

Momenteel leest u de documentatie voor het Wegenregister {description.ApiVersion}{string.Format(description.IsDeprecated ? ", deze API versie is niet meer ondersteund." : string.Empty)}

Een overzicht van alle bestaande versies kan u vinden in volgende lijst.

Versie  | Documentatie |
------- | ------------ |
");

            foreach (var version in provider.ApiVersionDescriptions)
                text.Append(
                    $"{version.GroupName}.0 | [https://weg.basisregisters.vlaanderen/{version.GroupName}](https://weg.basisregisters.vlaanderen/{description.GroupName})");

            return text.ToString();
        }
    }
}
