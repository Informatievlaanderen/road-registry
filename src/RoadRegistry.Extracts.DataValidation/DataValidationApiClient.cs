namespace RoadRegistry.Extracts.DataValidation;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public interface IDataValidationApiClient
{
    Task<string> RequestDataValidationAsync(UploadId uploadId, Stream blobStream, CancellationToken cancellationToken);
    Task<PollDeliveryResponse> PollDeliveryAsync(string deliveryId, CancellationToken cancellationToken);
    Task<GetDeliveryResultResponse> GetDeliveryResultAsync(string deliveryId, CancellationToken cancellationToken);
    Task<GetDeliveryArtifactsResponse> GetDeliveryArtifactsAsync(string deliveryId, CancellationToken cancellationToken);
}

public class DataValidationApiClient : IDataValidationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DataValidationOptions _options;
    private readonly IDataValidationTokenProvider _tokenProvider;

    public DataValidationApiClient(
        IHttpClientFactory httpClientFactory,
        DataValidationOptions options,
        IDataValidationTokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _tokenProvider = tokenProvider;
    }

    public async Task<string> RequestDataValidationAsync(UploadId uploadId, Stream blobStream, CancellationToken cancellationToken)
    {
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(_options.SpecificationCode), "specificationCode");
        formContent.Add(new StreamContent(blobStream), "delivery", "delivery.zip");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiBaseUrl.TrimEnd('/')}/deliveries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());
        request.Content = formContent;

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<CreateValidationJobResponse>(json, JsonOptions)!;
        return result.Id.ToString();
    }

    public async Task<PollDeliveryResponse> PollDeliveryAsync(string deliveryId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiBaseUrl.TrimEnd('/')}/deliveries/{deliveryId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PollDeliveryResponse>(json, JsonOptions)!;
    }

    public async Task<GetDeliveryResultResponse> GetDeliveryResultAsync(string deliveryId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiBaseUrl.TrimEnd('/')}/deliveries/{deliveryId}/result");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetDeliveryResultResponse>(json, JsonOptions)!;
    }

    public async Task<GetDeliveryArtifactsResponse> GetDeliveryArtifactsAsync(string deliveryId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiBaseUrl.TrimEnd('/')}/deliveries/{deliveryId}/artifacts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetDeliveryArtifactsResponse>(json, JsonOptions)!;
    }

    private sealed record CreateValidationJobResponse([property: JsonPropertyName("id")] Guid Id);
}

public static class ValidationJobStatus
{
    public const string Received = "Received";
    public const string Processing = "Processing";
    public const string Processed = "Processed";
    public const string Error = "Error";
}

public static class ValidationResult
{
    public const string NotYetAvailable = "NotYetAvailable";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string ApprovedWithRemarks = "ApprovedWithRemarks";
    public const string AutomaticallyRejected = "AutomaticallyRejected";
}

public static class ControlResult
{
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class ValidationErrorType
{
    public const string Error = "Error";
    public const string Remark = "Remark";
}

public sealed record PollDeliveryResponse(
    string Status,
    string Title,
    string? Stage,
    string Result,
    string? ResultUri);

public sealed record GetDeliveryResultResponse(
    string SchemaVersion,
    string Result,
    IReadOnlyList<DeliveryControl> Controls);

public sealed record DeliveryControl(
    string Code,
    string Result,
    IReadOnlyList<DeliveryValidationError> ValidationErrors);

public sealed record DeliveryValidationError(
    string TestId,
    string Clarification,
    string TestDescription,
    string ErrorType,
    string? Remark);

public sealed record GetDeliveryArtifactsResponse(
    IReadOnlyList<DeliveryArtifact> Artifacts);

public sealed record DeliveryArtifact(
    string Type,
    string Url);


public static class DeliveryArtifactType
{
    public const string QualityReport = "QualityReport";
    public const string ErrorGeometries = "ErrorGeometries";
}
