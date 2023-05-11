namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Attributes;

using Api.Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;

public class ApiAuthAttributeTests
{
    [Fact]
    public void ItShouldAccept()
    {
        var json = "{\"clientname\": \"test\", \"apikey\": \"E9536EA5-165C-461A-8E4A-641D0A65154B\", \"metadata\": { \"wraccess\": \"true\", \"syncaccess\": \"false\", \"tickets\": \"false\" }}";
        var deserializedApiToken = JsonConvert.DeserializeObject<ApiKeyAuthAttribute.ApiToken>(json);
    }

    [Fact]
    public void ItShouldDeserialize()
    {
        var serializedApiToken = "eyJhcGlLZXkiOiJFOTUzNkVBNS0xNjVDLTQ2MUEtOEU0QS02NDFEMEE2NTE1NEIiLCJjbGllbnROYW1lIjoiSm9obiBEb2UiLCJtZXRhZGF0YSI6eyJ3ckFjY2VzcyI6dHJ1ZSwic3luY0FjY2VzcyI6ZmFsc2UsInRpY2tldHNBY2Nlc3MiOmZhbHNlfX0=";

        var apiToken = ApiKeyAuthAttribute.ApiToken.FromBase64String(serializedApiToken);
    }

    [Fact]
    public void ItShouldSerialize()
    {
        var apiToken = new ApiKeyAuthAttribute.ApiToken("E9536EA5-165C-461A-8E4A-641D0A65154B", "John Doe", new ApiKeyAuthAttribute.ApiTokenMetadata(true, false, false))
        {
            Revoked = false
        };

        var serializedApiToken = apiToken.ToBase64String();
    }
}
