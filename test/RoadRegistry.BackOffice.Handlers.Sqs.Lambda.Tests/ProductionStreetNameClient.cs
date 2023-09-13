namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using RoadRegistry.StreetName;

public class ProductionStreetNameClient : IStreetNameClient
{
    private readonly IStreetNameClient _client;

    public ProductionStreetNameClient(IHttpClientFactory httpClientFactory)
    {
        _client = new StreetNameApiClient(httpClientFactory, new StreetNameRegistryOptions
        {
            StreetNameRegistryBaseUrl = "https://api.basisregisters.vlaanderen.be"
        });
    }

    public Task<StreetNameItem> GetAsync(int id, CancellationToken cancellationToken)
    {
        return _client.GetAsync(id, cancellationToken);
    }
}
