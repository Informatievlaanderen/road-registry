namespace Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Newtonsoft.Json;
    //TODO-rik add Map() overload in original package
    public class RoadRegistrySqsJsonMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }

        public RoadRegistrySqsJsonMessage()
            : this(string.Empty, string.Empty)
        { }

        public RoadRegistrySqsJsonMessage(string type, string data)
        {
            Type = type;
            Data = data;
        }

        public object Map(JsonSerializer serializer)
        {
            var assembly = GetAssemblyNameContainingType(Type);
            var type = assembly?.GetType(Type);

            using (var reader = new StringReader(Data))
            {
                return serializer.Deserialize(reader, type!);
            }
        }

        public static SqsJsonMessage Create<T>([DisallowNull] T message, JsonSerializer serializer)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            return new SqsJsonMessage(message.GetType().FullName!, serializer.Serialize(message));
        }

        private static Assembly? GetAssemblyNameContainingType(string typeName) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(x => new
            {
                Assembly = x,
                Type = x.GetType(typeName, false, true)
            })
            .Where(x => x.Type != null)
            .Select(x => x.Assembly)
            .FirstOrDefault();
    }
}
