namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System;
using System.Configuration;
using Configuration;
using Controllers.Attributes;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NisCodeService.Abstractions;
using NisCodeService.Proxy.HttpProxy;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAcmIdmAuth(
        this IServiceCollection services,
        OAuth2IntrospectionOptions oAuth2IntrospectionOptions,
        OpenIdConnectOptions openIdConnectOptions)
    {
        services
            .AddHttpProxyNisCodeService()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters =
                        new RoadRegistryTokenValidationParameters(openIdConnectOptions);
                })
            .AddOAuth2Introspection(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = oAuth2IntrospectionOptions.ClientId;
                options.ClientSecret = oAuth2IntrospectionOptions.ClientSecret;
                options.Authority = oAuth2IntrospectionOptions.Authority;
                options.IntrospectionEndpoint = oAuth2IntrospectionOptions.IntrospectionEndpoint;
            });
        return services;
    }

    public static AuthenticationBuilder AddApiKeyAuth(
        this IServiceCollection services)
    {
        return services
                .AddSingleton<IApiKeyAuthenticator, ApiKeyAuthenticator>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = ApiKeyDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
                })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.AuthenticationScheme, options => { })
            ;
    }

    public static IServiceCollection AddHttpProxyNisCodeService(this IServiceCollection services)
    {
        services
            .AddHttpClient<INisCodeService, HttpProxyNisCodeService>((sp, c) =>
            {
                var nisCodeServiceUrl = sp.GetRequiredService<IConfiguration>().GetValue<string>("NisCodeServiceUrl");
                if (string.IsNullOrWhiteSpace(nisCodeServiceUrl))
                {
                    throw new ConfigurationErrorsException("Configuration should have a value for \"NisCodeServiceUrl\".");
                }

                c.BaseAddress = new Uri(nisCodeServiceUrl.TrimEnd('/'));
            });
        return services;
    }
}
