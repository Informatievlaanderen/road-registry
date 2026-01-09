namespace RoadRegistry.Tests.BackOffice;

using System.Text;
using RoadRegistry.BackOffice;

public static class NationalRoadNumbers
{
    public static readonly NationalRoadNumber[] All = ReadAllFromResource();

    private static NationalRoadNumber[] ReadAllFromResource()
    {
        using var stream = EmbeddedResourceReader.Read("ident2.txt");
        if (stream is null)
        {
            return Array.Empty<NationalRoadNumber>();
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var numbers = new List<NationalRoadNumber>(747);
        var line = reader.ReadLine();
        while (line != null)
        {
            numbers.Add(NationalRoadNumber.Parse(line));
            line = reader.ReadLine();
        }

        return numbers.ToArray();
    }
}
