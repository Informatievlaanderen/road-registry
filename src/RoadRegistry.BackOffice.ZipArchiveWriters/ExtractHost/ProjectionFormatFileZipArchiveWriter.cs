namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Extracts;
using Microsoft.EntityFrameworkCore;

public class ProjectionFormatFileZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    public static readonly string StaticFileContents = @"PROJCS[""Belge_Lambert_1972"",GEOGCS[""GCS_Belge_1972"",DATUM[""D_Belge_1972"",SPHEROID[""International_1924"",6378388.0,297.0]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]],PROJECTION[""Lambert_Conformal_Conic""],PARAMETER[""False_Easting"",150000.01256],PARAMETER[""False_Northing"",5400088.4378],PARAMETER[""Central_Meridian"",4.367486666666666],PARAMETER[""Standard_Parallel_1"",49.8333339],PARAMETER[""Standard_Parallel_2"",51.16666723333333],PARAMETER[""Latitude_Of_Origin"",90.0],UNIT[""Meter"",1.0]]";
    private readonly Encoding _encoding;
    private readonly string _filename;

    public ProjectionFormatFileZipArchiveWriter(string filename, Encoding encoding)
    {
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
        CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (context == null) throw new ArgumentNullException(nameof(context));
        var prjEntry = archive.CreateEntry(_filename);
        await using (var prjEntryStream = prjEntry.Open())
        await using (var prjEntryStreamWriter = new StreamWriter(prjEntryStream, _encoding))
        {
            await prjEntryStreamWriter.WriteAsync(StaticFileContents);
            await prjEntryStreamWriter.FlushAsync();
        }
    }
}