namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

public static class NumberedRoadNumbers
{
    public static readonly NumberedRoadNumber[] All = ReadAllFromResource();

    private static NumberedRoadNumber[] ReadAllFromResource()
    {
        using (var stream = Assembly
                   .GetAssembly(typeof(NumberedRoadNumbers))
                   .GetManifestResourceStream(typeof(NumberedRoadNumbers), "ident8.txt"))
        {
            if (stream != null)
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var numbers = new List<NumberedRoadNumber>(5807);
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        numbers.Add(NumberedRoadNumber.Parse(line));
                        line = reader.ReadLine();
                    }

                    return numbers.ToArray();
                }

            return Array.Empty<NumberedRoadNumber>();
        }
    }
}
