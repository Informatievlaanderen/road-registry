using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Framework
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    public class SimpleQueueCommand
    {
        public SimpleQueueCommand(object body)
        {
            Type = $"{body.GetType().FullName}, {body.GetType().Assembly.FullName}";
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
