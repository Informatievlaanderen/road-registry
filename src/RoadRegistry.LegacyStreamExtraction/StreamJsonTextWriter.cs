namespace RoadRegistry.LegacyStreamExtraction
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal class StreamJsonTextWriter : IDisposable
    {
        public StreamJsonTextWriter(JsonTextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.DateTime,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
        }

        public JsonTextWriter Writer { get; }
        public JsonSerializer Serializer { get; }

        public void Dispose()
        {
            ((IDisposable)Writer).Dispose();
        }

        public async Task WriteStream(string stream, IEnumerable<object> events)
        {
            await Writer.WriteStartObjectAsync(); // begin stream

            await Writer.WritePropertyNameAsync("Stream");
            await Writer.WriteValueAsync(stream);

            await Writer.WritePropertyNameAsync("Events");

            await Writer.WriteStartArrayAsync(); // begin events

            foreach(var @event in events)
            {
                await Writer.WriteStartObjectAsync(); // begin event
                await Writer.WritePropertyNameAsync(@events.GetType().Name);
                Serializer.Serialize(Writer, @event);
                await Writer.WriteEndObjectAsync(); // end event
            }

            await Writer.WriteEndArrayAsync(); // end events

            await Writer.WriteEndObjectAsync(); // end stream
        }
    }
}