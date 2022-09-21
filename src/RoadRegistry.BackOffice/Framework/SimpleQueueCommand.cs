using Newtonsoft.Json;

namespace RoadRegistry.BackOffice.Framework
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    public class SimpleQueueCommand
    {
        public SimpleQueueCommand(object body) : this(body, $"{body.GetType().FullName}, {body.GetType().Assembly.FullName}")
        {
        }

        [JsonConstructor]
        public SimpleQueueCommand(object body, string type)
        {
            Type = type;
            Body = body;
        }

        public string Type { get; init; }
        public object Body { get; init; }

        public object ToActualType()
        {
            var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            return ToActualType(jsonSerializerSettings);
        }

        public object ToActualType(JsonSerializerSettings jsonSerializerSettings) => JsonConvert.DeserializeObject(Body.ToString(), System.Type.GetType(Type), jsonSerializerSettings);
    }
}
