namespace RoadRegistry.Projections.Tests.Projections;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

/// <summary>
/// Helpers around how a geometry actually travels to and from SQL Server, so the read-model projection tests can assert
/// on what ends up in the column rather than on the in-memory NTS instance.
/// </summary>
public static class SqlServerGeometry
{
    // Both directions go through the very readers/writers the EF Core SQL Server spatial provider uses.
    private static readonly SqlServerBytesWriter Writer = new() { IsGeography = false };
    private static readonly SqlServerBytesReader Reader = new() { IsGeography = false };

    /// <summary>
    /// Asserts that serializing the geometry for a SQL Server <c>geometry</c> column yields a plain 2D value: neither
    /// the Z nor the M flag is set, so <c>[GEOMETRIE].HasZ</c> and <c>[GEOMETRIE].HasM</c> come back 0.
    /// </summary>
    /// <remarks>
    /// Asserting that every <see cref="Coordinate"/> has a NaN Z/M is not enough: a coordinate sequence that declares a
    /// Z and an M ordinate but leaves them NaN passes that check, while
    /// <see cref="SqlServerBytesWriter"/> still sets both flags and SQL Server stores the geometry as 3D/measured.
    /// </remarks>
    public static void AssertIs2D(Geometry geometry)
    {
        Assert.NotNull(geometry);

        var (hasZ, hasM) = Ordinates(geometry);
        Assert.False(hasZ, "Geometry must be stored without a Z ordinate.");
        Assert.False(hasM, "Geometry must be stored without an M ordinate.");
    }

    /// <summary>
    /// The geometry as EF Core hands it back after a round trip through a SQL Server <c>geometry</c> column.
    /// <see cref="SqlServerBytesReader"/> always builds coordinate sequences that declare a Z and an M ordinate (both
    /// NaN), even for a column holding plain 2D values — so anything projected from a stored geometry has to be forced
    /// back to 2D before it is written again. The in-memory provider the projection scenarios run on hands back the
    /// very instance that was written, which hides that.
    /// </summary>
    public static Geometry AsReadFromSqlServer(Geometry geometry)
    {
        return Reader.Read(Writer.Write(geometry));
    }

    // SQL Server's serialization format: int32 SRID, byte version, byte properties. Bit 0 of the properties byte is
    // the Z flag, bit 1 the M flag.
    private static (bool HasZ, bool HasM) Ordinates(Geometry geometry)
    {
        var bytes = Writer.Write(geometry);
        var properties = bytes[5];
        return ((properties & 0x01) != 0, (properties & 0x02) != 0);
    }
}
