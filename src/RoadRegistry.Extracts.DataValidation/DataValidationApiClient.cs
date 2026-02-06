namespace RoadRegistry.Extracts.DataValidation;

public interface IDataValidationApiClient
{
    Task<string> RequestDataValidationAsync(CancellationToken cancellationToken);
}

public class DataValidationApiClient : IDataValidationApiClient
{
    private readonly HttpClient _httpClient;

    public DataValidationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> RequestDataValidationAsync(CancellationToken cancellationToken)
    {
        //TODO-pr implement send archive to datavalidation

        return Guid.NewGuid().ToString();
    }
}
