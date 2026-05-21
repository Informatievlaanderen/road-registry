namespace RoadRegistry.Extracts.DataValidation;

using RoadRegistry.BackOffice;

public class DataValidationOptions : IHasConfigurationKey
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string SpecificationCode { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenEndPoint { get; set; } = string.Empty;

    public string GetConfigurationKey()
    {
        return "DataValidationOptions";
    }
}
