namespace RoadRegistry.BackOffice.Handlers.Extensions;

public static class StringExtensions
{
    public static int GetIdentifierFromPuri(this string puri)
    {
        if (puri != null)
        {
            var lastPart = puri.Split('/').Last();
            if (int.TryParse(lastPart, out var identifier))
            {
                return identifier;
            }
        }

        return 0;
    }

    public static string GetIdentifierPartFromPuri(this string puri)
    {
        return puri.Split('/').Last();
    }
}
