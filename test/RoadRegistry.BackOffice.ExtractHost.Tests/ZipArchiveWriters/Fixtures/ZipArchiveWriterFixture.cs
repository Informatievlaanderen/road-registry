namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters.Fixtures;

using Extracts;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts;

public abstract class ZipArchiveWriterFixture
{
    public ZipArchiveWriterFixture(WKTReader wktReader)
    {
        try
        {
            FileContent = File.ReadAllText(FileInfo.FullName);

            //var reader = new WKTReader(new NtsGeometryServices(
            //        GeometryConfiguration.GeometryFactory.PrecisionModel,
            //        GeometryConfiguration.GeometryFactory.SRID
            //    )
            //);

            var fileReader = new WKTFileReader(FileInfo, wktReader);
            Result = fileReader.Read();
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    public Exception Exception { get; }
    public string FileContent { get; }
    public abstract FileInfo FileInfo { get; }
    public abstract RoadNetworkExtractAssemblyRequest Request { get; }
    public IEnumerable<Geometry> Result { get; }
}