namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Configuration;
using Controllers.Attributes;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAcmIdmAuthentication(
        this IServiceCollection services,
        OAuth2IntrospectionOptions oAuth2IntrospectionOptions,
        OpenIdConnectOptions openIdConnectOptions)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationSchemes.Bearer;
                options.DefaultAuthenticateScheme = AuthenticationSchemes.Bearer;
                options.DefaultChallengeScheme = AuthenticationSchemes.Bearer;
            })
            .AddOAuth2Introspection(AuthenticationSchemes.Bearer, options =>
            {
                options.ClientId = oAuth2IntrospectionOptions.ClientId;
                options.ClientSecret = oAuth2IntrospectionOptions.ClientSecret;
                options.Authority = oAuth2IntrospectionOptions.Authority;
                options.IntrospectionEndpoint = oAuth2IntrospectionOptions.IntrospectionEndpoint;
            })
            .AddJwtBearer(AuthenticationSchemes.JwtBearer, options =>
            {
                options.TokenValidationParameters = new RoadRegistryTokenValidationParameters(openIdConnectOptions);
                options.Events ??= new JwtBearerEvents();
                options.Events.OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();

                    var authHeaderPrefix = $"{AuthenticationSchemes.JwtBearer} ";
                    if (authHeader.StartsWith(authHeaderPrefix, StringComparison.Ordinal))
                    {
                        context.Token = authHeader[authHeaderPrefix.Length..];
                    }
                    else
                    {
                        context.NoResult();
                    }

                    return Task.CompletedTask;
                };
            })
            ;

        return services;
    }

    public static IServiceCollection AddApiKeyAuth(this IServiceCollection services)
    {
        services
            .AddSingleton<IApiTokenReader>(sp =>
            {
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                if (hostEnvironment.IsDevelopment())
                {
                    return new ConfigurationApiTokenReader(sp.GetRequiredService<IConfiguration>(), "RoadRegistry Development ApiKey Client");
                }

                return new DynamoDbApiTokenReader(sp.GetRequiredService<AmazonDynamoDBClient>());
            })
            .AddSingleton<IApiKeyAuthenticator, ApiKeyAuthenticator>()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationSchemes.ApiKey;
                options.DefaultAuthenticateScheme = AuthenticationSchemes.ApiKey;
                options.DefaultChallengeScheme = AuthenticationSchemes.ApiKey;
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(AuthenticationSchemes.ApiKey, options => { })
            ;
        return services;
    }
}
