namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Text;
using Newtonsoft.Json;

public record ApiToken([JsonProperty("apikey")] string ApiKey, [JsonProperty("clientname")] string ClientName, [JsonProperty("metadata")] ApiTokenMetadata Metadata)
{
    [JsonIgnore] public bool Revoked { get; set; }

    public static ApiToken FromBase64String(string s)
    {
        var bytes = Convert.FromBase64String(s);
        var json = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject<ApiToken>(json);
    }

    public string ToBase64String()
    {
        return ToBase64String(this);
    }

    public static string ToBase64String(ApiToken apiToken)
    {
        var serializedApiToken = JsonConvert.SerializeObject(apiToken);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedApiToken));
    }
}