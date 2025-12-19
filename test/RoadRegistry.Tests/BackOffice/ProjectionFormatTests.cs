namespace RoadRegistry.Tests.BackOffice;

using System.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.Extracts;

public class ProjectionFormatTests
{
    public static IEnumerable<object[]> ProjectionFormatCases
    {
        get
        {
            yield return new object[] { true, ProjectionFormat.BelgeLambert1972.Content };
            yield return new object[] { false, @"PROJCS[""DHDN / 3-degree Gauss zone 1 (deprecated)"",GEOGCS[""DHDN"",DATUM[""D_Deutsches_Hauptdreiecksnetz"",SPHEROID[""Bessel_1841"",6377397.155,299.1528128]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",3],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",1500000],PARAMETER[""false_northing"",0],UNIT[""Meter"",1]]" };
            yield return new object[] { false, @"PROJCS[""Belge 1972 / Belgian Lambert 72"",GEOGCS[""Belge 1972"",DATUM[""D_Belge_1972"",SPHEROID[""International_1924"",6378388,297]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Lambert_Conformal_Conic""],PARAMETER[""standard_parallel_1"",51.16666723333333],PARAMETER[""standard_parallel_2"",49.8333339],PARAMETER[""latitude_of_origin"",90],PARAMETER[""central_meridian"",4.367486666666666],PARAMETER[""false_easting"",150000.013],PARAMETER[""false_northing"",5400088.438],UNIT[""Meter"",1]]" };
        }
    }

    [Fact]
    public void ContentCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ProjectionFormat(null));
    }

    [Fact]
    public void IsBelgeLambert1972DoesNotThrowGivenInvalidContent()
    {
        var sut = new ProjectionFormat("invalid");

        Assert.False(sut.IsBelgeLambert1972());
    }

    [Fact]
    public void IsBelgeLambert1972DoesNotThrowGivenValidContentButUnknownFormat()
    {
        var sut = new ProjectionFormat("GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]]");

        Assert.False(sut.IsBelgeLambert1972());
    }

    [Theory]
    [MemberData(nameof(ProjectionFormatCases))]
    public void IsBelgeLambert1972ReturnsExpectedResult(bool expected, string content)
    {
        var sut = new ProjectionFormat(content);

        Assert.Equal(expected, sut.IsBelgeLambert1972());
    }

    [Fact]
    public void ReadCanReadFromStream()
    {
        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream, Encoding.Default, leaveOpen: true))
        using (var reader = new StreamReader(stream, Encoding.Default, leaveOpen: true))
        {
            writer.Write(ProjectionFormat.BelgeLambert1972.Content);
            writer.Flush();

            stream.Position = 0;

            var result = ProjectionFormat.Read(reader);

            Assert.Equal(ProjectionFormat.BelgeLambert1972.Content, result.Content);
        }
    }

    [Fact]
    public void ReadCanReadInvalidValueFromStream()
    {
        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream, Encoding.Default, leaveOpen: true))
        using (var reader = new StreamReader(stream, Encoding.Default, leaveOpen: true))
        {
            const string expectedContent = "invalid";
            writer.Write(expectedContent);
            writer.Flush();

            stream.Position = 0;

            var result = ProjectionFormat.Read(reader);

            Assert.Equal(expectedContent, result.Content);
        }
    }
}
