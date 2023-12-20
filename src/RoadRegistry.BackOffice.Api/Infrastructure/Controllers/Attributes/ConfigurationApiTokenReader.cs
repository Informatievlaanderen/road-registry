namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ConfigurationApiTokenReader : IApiTokenReader
{
    private readonly IConfiguration _configuration;
    private readonly string _clientName;

    public ConfigurationApiTokenReader(IConfiguration configuration, string clientName)
    {
        _configuration = configuration;
        _clientName = clientName;
    }

    public Task<ApiToken> ReadAsync(string apiKey)
    {
        var apiKeys = _configuration
            .GetSection("ApiKeys:Road")
            .GetChildren()
            .Select(c => c.Value)
            .ToArray();
        if (apiKeys.Contains(apiKey))
        {
            return Task.FromResult(new ApiToken(apiKey, _clientName, new ApiTokenMetadata(true)));
        }

        return Task.FromResult<ApiToken>(null);
    }
}
