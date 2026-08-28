namespace RoadRegistry.Projections.Tests.Projections;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.WmsWfsV2.Schema;
using RoadRegistry.WmsWfsV2.Schema.Records;

// Covers what the bulk insert path decides before it ever reaches SQL Server: which entity types it will take, and how
// their values are flattened. The copy itself needs a real server and lives with the integration tests.
//
// The context is configured for SQL Server but never opened - the model and the change tracker are all these need, and
// building either does not connect.
public class SqlServerBulkInserterTests
{
    private static WmsWfsV2Context CreateContext()
    {
        return new WmsWfsV2Context(new DbContextOptionsBuilder<WmsWfsV2Context>()
            .UseSqlServer("Server=not-connected;Database=none;", o => o.UseNetTopologySuite())
            .Options);
    }

    private static DerivedRoadSegmentRecord DerivedRow(int roadSegmentId)
    {
        return new DerivedRoadSegmentRecord
        {
            WS_OIDN = roadSegmentId,
            GEOMETRIE = new LineString([new Coordinate(0, 0), new Coordinate(1, 1)]) { SRID = 31370 },
            STATUS = 4,
            LBLSTATUS = "gerealiseerd",
            CREATIE = DateTimeOffset.UnixEpoch,
            VERSIE = DateTimeOffset.UnixEpoch
        };
    }

    [Fact]
    public void GivenEnoughRowsOfATypeWithAnIdentityKey_ThenTheyAreCollected()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 10; i++)
        {
            context.DerivedRoadSegments.Add(DerivedRow(i));
        }

        var batches = SqlServerBulkInserter.Collect(context, threshold: 10);

        batches.Should().HaveCount(1);
        batches[0].Rows.Should().HaveCount(10);
        batches[0].DestinationTableName.Should().Be($"[{WellKnownSchemas.WmsWfsV2Schema}].[{DerivedRoadSegmentRecordConfiguration.TableName}]");
    }

    [Fact]
    public void GivenFewerRowsThanTheThreshold_ThenNothingIsCollected()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 9; i++)
        {
            context.DerivedRoadSegments.Add(DerivedRow(i));
        }

        SqlServerBulkInserter.Collect(context, threshold: 10).Should().BeEmpty();
    }

    // The whole safety argument for copying before EF applies the batch's deletes is that an inserted row cannot
    // collide with one being deleted. That only holds for a store-generated key, so a table with an application
    // assigned key has to stay on the EF path however many rows it has.
    [Fact]
    public void GivenATypeWithAnApplicationAssignedKey_ThenItIsNeverCollected()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 100; i++)
        {
            context.RoadSegments.Add(new RoadSegmentRecord
            {
                WS_OIDN = i,
                CREATIE = DateTimeOffset.UnixEpoch,
                VERSIE = DateTimeOffset.UnixEpoch
            });
        }

        SqlServerBulkInserter.Collect(context, threshold: 10).Should().BeEmpty();
    }

    [Fact]
    public void TheIdentityColumnIsNotCopied()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 10; i++)
        {
            context.DerivedRoadSegments.Add(DerivedRow(i));
        }

        var batch = SqlServerBulkInserter.Collect(context, threshold: 10).Single();

        batch.Columns.Should().NotContain(nameof(DerivedRoadSegmentRecord.WS_TEMPID));
        batch.Columns.Should().Contain(nameof(DerivedRoadSegmentRecord.WS_OIDN));
    }

    // SqlBulkCopy cannot convert an NTS geometry itself; it has to travel as the serialized form the SQL Server
    // spatial types use, which is what the EF provider writes for a geometry column.
    [Fact]
    public void GeometryIsFlattenedToItsSqlServerBinaryForm()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 10; i++)
        {
            context.DerivedRoadSegments.Add(DerivedRow(i));
        }

        var batch = SqlServerBulkInserter.Collect(context, threshold: 10).Single();
        var geometryOrdinal = batch.Columns.ToList().IndexOf(nameof(DerivedRoadSegmentRecord.GEOMETRIE));

        geometryOrdinal.Should().BeGreaterThanOrEqualTo(0);
        batch.Rows[0][geometryOrdinal].Should().BeOfType<byte[]>()
            .Which.Should().NotBeEmpty();
    }

    [Fact]
    public void NullsSurviveAsNulls()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 10; i++)
        {
            var row = DerivedRow(i);
            row.LBLMORF = null;
            context.DerivedRoadSegments.Add(row);
        }

        var batch = SqlServerBulkInserter.Collect(context, threshold: 10).Single();
        var ordinal = batch.Columns.ToList().IndexOf(nameof(DerivedRoadSegmentRecord.LBLMORF));

        batch.Rows[0][ordinal].Should().BeNull();
    }

    [Fact]
    public void CollectedEntriesAreOnlyDetachedOnceTheCallerSaysSo()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 10; i++)
        {
            context.DerivedRoadSegments.Add(DerivedRow(i));
        }

        var batches = SqlServerBulkInserter.Collect(context, threshold: 10);
        context.ChangeTracker.Entries().Count(x => x.State == EntityState.Added).Should().Be(10);

        SqlServerBulkInserter.Detach(context, batches);

        context.ChangeTracker.Entries().Should().BeEmpty();
    }
}
