namespace RoadRegistry.BackOffice;

public readonly struct StreetNamePuri
{
    private readonly int _id;

    public StreetNamePuri(int id)
    {
        _id = id;
    }

    public override string ToString()
    {
        return _id > 0 ? $"https://data.vlaanderen.be/id/straatnaam/{_id}" : _id.ToString();
    }
}
