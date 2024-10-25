namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Console;

using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NodaTime;
using RoadNetworks;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;

class Program
{
    static async Task Main(string[] args)
    {
        var sqsRequest = new CreateRoadNetworkSnapshotSqsRequest
        {
            Request = new CreateRoadNetworkSnapshotRequest()
        };

        await RunSqsRequest(sqsRequest);
    }

    private static async Task RunSqsRequest(SqsRequest sqsRequest)
    {
        var function = new Function();

        var clock = SystemClock.Instance;
        sqsRequest.ProvenanceData = new ProvenanceData(new Provenance(clock.GetCurrentInstant(), Application.RoadRegistry, new Reason(""), new Operator(""), Modification.Unknown, Organisation.Agiv));

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var sqsJsonMessage = SqsJsonMessage.Create(sqsRequest, jsonSerializerSettings);

        var sqsEvent = new SQSEvent
        {
            Records =
            [
                new SQSEvent.SQSMessage
                {
                    Attributes = new Dictionary<string, string>
                    {
                        { "MessageGroupId", string.Empty }
                    },
                    Body = JsonConvert.SerializeObject(sqsJsonMessage, jsonSerializerSettings)
                }
            ]
        };

        var lambdaSerializer = new JsonSerializerSettings
        {
            ContractResolver = new CamelCaseExceptDictionaryKeysResolver()
        };
        var sqsEventAsJObject = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(sqsEvent, lambdaSerializer), lambdaSerializer);

        await function.Handler(sqsEventAsJObject, new TestLambdaContext());
    }

    private sealed class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);
            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}
