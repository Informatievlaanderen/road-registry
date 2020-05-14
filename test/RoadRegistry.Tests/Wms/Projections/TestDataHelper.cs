namespace RoadRegistry.Wms.Projections
{
    using System.IO;
    using System.Threading.Tasks;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public class TestDataHelper
    {
        public async Task<T> EventFromFileAsync<T>(int number)
        {
            var json = await File.ReadAllTextAsync($"Wms/Projections/TestData/importedRoadSegment.{number}.json");

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<RoadSegmentDenormTestRecord> ExpectedWegsegmentDeNormFromFileAsync(int number)
        {
            var lines = await File.ReadAllLinesAsync($"Wms/Projections/TestData/expected.{number}.csv");

            // todo: escape commas in csv (geometrie2D is not _really_ index 34)
            // todo: use real csv reader
            var split = lines[1].Split(',');

            return new RoadSegmentDenormTestRecord
            {
                Id = int.Parse(split[0]),
                Geometrie = WKBReader.HexToBytes(split[7].Substring(2)),
                Geometrie2D = WKBReader.HexToBytes(split[34].Substring(2)),
            };
        }

        public class RoadSegmentDenormTestRecord
        {
            public int Id { get; set; }

            public byte[] Geometrie { get; set; }
            public byte[] Geometrie2D { get; set; }
        }
    }
}
