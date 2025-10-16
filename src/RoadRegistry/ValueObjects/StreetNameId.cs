namespace RoadRegistry.BackOffice;
using System;
using System.Linq;
using Extensions;

public readonly struct StreetNameId
{
    private const string Namespace = "https://data.vlaanderen.be/id/straatnaam/";

    private readonly string _value;

    public StreetNameId(int objectId)
    {
        ObjectId = objectId;
        _value = ObjectId > 0 ? $"{Namespace}{ObjectId}" : ObjectId.ToString();
    }

    public StreetNameId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!AcceptsValue(value))
        {
            throw new ArgumentException("The value is not a well known street name identifier", nameof(value));
        }

        var id = value.Split('/').Last();
        if (!int.TryParse(id, out var objectId))
        {
            throw new ArgumentException("The value does not contain a valid object id", nameof(value));
        }

        ObjectId = objectId;
        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.ContainsWhitespace() && value.StartsWith(Namespace);
    }

    public int ObjectId { get; }

    public override string ToString()
    {
        return _value;
    }

    public StreetNameLocalId ToStreetNameLocalId()
    {
        return new StreetNameLocalId(ObjectId);
    }
    
    public static implicit operator string(StreetNameId instance)
    {
        return instance.ToString();
    }
}
