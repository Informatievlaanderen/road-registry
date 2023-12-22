using System;

namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts
{
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;

    internal partial class ExtractsTests
    {
        protected HttpClient ApiClient { get; }

        public ExtractsTests(IConfiguration configuration)
        {
            ApiClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetRequiredValue<string>("BackOfficeApiBaseUrl"))
            };
        }
    }
}
