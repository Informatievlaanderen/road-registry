namespace RoadRegistry.BackOffice;

using System;
using System.Linq;

public readonly struct StreetNamePuri
{
    public StreetNamePuri(int id)
    {
        ObjectId = id;
    }

    public int ObjectId { get; }

    public override string ToString()
    {
        return ObjectId > 0 ? $"https://data.vlaanderen.be/id/straatnaam/{ObjectId}" : ObjectId.ToString();
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
