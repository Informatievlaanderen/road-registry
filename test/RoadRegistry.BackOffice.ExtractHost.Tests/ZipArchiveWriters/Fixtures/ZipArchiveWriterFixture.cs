namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters.Fixtures;

using Extracts;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

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

    public IEnumerable<Geometry> Result { get; }

    public abstract RoadNetworkExtractAssemblyRequest Request { get; }

    public Exception Exception { get; }

    public abstract FileInfo FileInfo { get; }

    public string FileContent { get; }
}
