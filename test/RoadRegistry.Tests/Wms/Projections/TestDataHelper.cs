namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
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

        public async Task<Geometry> ExpectedGeometry(int number)
        {
            var reader = new SqlServerBytesReader
            {
                HandleOrdinates = Ordinates.AllOrdinates,
                HandleSRID = true
            };

            var lines = await File.ReadAllLinesAsync($"Wms/Projections/TestData/expected.{number}.csv");
            var split = lines[1].Split(',');
            return reader.Read(StringToByteArray(split[7].Substring(2)));
        }

        public async Task<Geometry> ExpectedGeometry2D(int number)
        {
            var reader = new SqlServerBytesReader
            {
                HandleOrdinates = Ordinates.AllOrdinates,
                HandleSRID = true
            };

            var lines = await File.ReadAllLinesAsync($"Wms/Projections/TestData/expected.{number}.csv");
            var split = lines[1].Split(',');
            return reader.Read(StringToByteArray(split[34].Substring(2)));
        }

        private static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public class RoadSegmentDenormTestRecord
        {
            public int Id { get; set; }

            public byte[] Geometrie { get; set; }
            public byte[] Geometrie2D { get; set; }
        }
    }
}
