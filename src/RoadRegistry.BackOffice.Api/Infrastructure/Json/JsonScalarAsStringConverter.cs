namespace RoadRegistry.BackOffice.Api.Infrastructure.Json;

using System;
using System.Globalization;
using Newtonsoft.Json;

/// <summary>
///     Reads a JSON scalar into a string property whatever its token type is, so that a value of the wrong type is
///     answered by the parameter's own validation instead of by the serializer.
/// </summary>
// A number or a boolean where the name of an enumeration is expected is an invalid value for that parameter, not a
// body that cannot be read: 'geometriemethode: 123' has to come back as "De parameter 'geometriemethode' heeft een
// ongeldige waarde." Newtonsoft turns such a token into its text by itself, but only implicitly, and what a host
// does with it depends on the serializer that host configures - the apis in front of this one do not all configure
// the same, and on one of them a numeric value ended up as a 500 instead (GAWR-7870). The conversion is done here
// instead, where it is this contract's own and the same everywhere.
//
// An object or an array is a different matter. There is no text to read them as, so they stay what they were: a body
// that does not match the contract, which the serializer reports and the api answers as invalid JSON.
public sealed class JsonScalarAsStringConverter : JsonConverter<string?>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        throw new NotSupportedException($"{nameof(JsonScalarAsStringConverter)} only reads.");
    }

    public override string? ReadJson(
        JsonReader reader,
        Type objectType,
        string? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            // Left to the serializer: a required parameter that is null or missing is already answered as such.
            case JsonToken.Null:
            case JsonToken.Undefined:
                return null;

            case JsonToken.StartObject:
            case JsonToken.StartArray:
                throw new JsonSerializationException(
                    $"Unexpected token {reader.TokenType} while reading a value of type {objectType.Name}.");

            default:
                return reader.Value switch
                {
                    null => null,
                    string text => text,
                    // Written the way JSON writes them, so 'true' reads back as the text that was sent.
                    bool boolean => boolean ? "true" : "false",
                    IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                    var value => value.ToString()
                };
        }
    }
}
