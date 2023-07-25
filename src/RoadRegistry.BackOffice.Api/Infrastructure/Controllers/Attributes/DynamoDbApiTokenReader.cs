namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

public class DynamoDbApiTokenReader : IApiTokenReader
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;

    public DynamoDbApiTokenReader(AmazonDynamoDBClient dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<ApiToken> ReadAsync(string apiKey)
    {
        var request = new GetItemRequest
        {
            TableName = "basisregisters-api-gate-keys",
            Key = new Dictionary<string, AttributeValue> { { nameof(ApiToken.ApiKey), new AttributeValue(apiKey) } },
            AttributesToGet = new List<string>
            {
                nameof(ApiToken.ApiKey),
                nameof(ApiToken.ClientName),
                nameof(ApiToken.Revoked),
                nameof(ApiTokenMetadata.WrAccess),
            },
            ConsistentRead = true
        };

        var response = await _dynamoDbClient.GetItemAsync(request);
        if (!response.IsItemSet)
        {
            return null;
        }

        response.Item.TryGetValue(nameof(ApiToken.ClientName), out var clientName);
        response.Item.TryGetValue(nameof(ApiToken.Revoked), out var revoked);
        response.Item.TryGetValue(nameof(ApiTokenMetadata.WrAccess), out var wrAccess);

        var hasWrAccess = wrAccess?.BOOL ?? false;
        var isRevoked = revoked?.BOOL ?? false;
        return new ApiToken(apiKey, clientName?.S ?? string.Empty, new ApiTokenMetadata(hasWrAccess))
        {
            Revoked = isRevoked
        };
    }
}
