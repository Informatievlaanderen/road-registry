namespace RoadRegistry.BackOffice;
using System;
using System.Linq;
using Extensions;

public readonly struct StreetNamePuri
{
    private const string Namespace = "https://data.vlaanderen.be/id/straatnaam/";

    public StreetNamePuri(int objectId)
    {
        ObjectId = objectId;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.ContainsWhitespace() && value.StartsWith(Namespace);
    }

    public int ObjectId { get; }

    public override string ToString()
    {
        return ObjectId > 0 ? $"{Namespace}{ObjectId}" : ObjectId.ToString();
    }

    public StreetNameLocalId ToStreetNameLocalId()
    {
        return new StreetNameLocalId(ObjectId);
    }

    public static StreetNamePuri FromValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var id = value.Split('/').Last();
        return new StreetNamePuri(int.Parse(id));
    }

    public static implicit operator string(StreetNamePuri instance)
    {
        return instance.ToString();
    }
}
