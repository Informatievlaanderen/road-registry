namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections.Framework;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

public class TestDataHelper
{
    private static SqlServerBytesReader CreateSqlServerBytesReader()
    {
        return new SqlServerBytesReader
        {
            HandleOrdinates = Ordinates.AllOrdinates,
            HandleSRID = true
        };
    }

    public async Task<T> EventFromFileAsync<T>(int number)
    {
        var json = await File.ReadAllTextAsync($"Projections/TestData/importedRoadSegment.{number}.json");

        return JsonConvert.DeserializeObject<T>(json);
    }

    public Geometry ExpectedGeometry(int number)
    {
        var record = ExpectedRoadSegment(number);
        return CreateSqlServerBytesReader().Read(StringToByteArray(record.geometrie.Substring(2)));
    }

    public Geometry ExpectedGeometry2D(int number)
    {
        var record = ExpectedRoadSegment(number);
        return CreateSqlServerBytesReader().Read(StringToByteArray(record.geometrie2D.Substring(2)));
    }

    public ExpectedWegsegmentRecord ExpectedRoadSegment(int number)
    {
        using (var streamReader = new StreamReader($"Projections/TestData/expected.{number}.csv"))
        using (var csv = new CsvTestDataReader(streamReader))
        {
            return csv.GetRecords<ExpectedWegsegmentRecord>().Single();
        }
    }

    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
}