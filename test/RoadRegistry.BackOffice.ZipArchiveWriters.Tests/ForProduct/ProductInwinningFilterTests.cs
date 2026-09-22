namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForProduct;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Product.Schema;
using RoadRegistry.Product.Schema.RoadSegments;

public class ProductInwinningFilterTests
{
    [Fact]
    public void NoneExcludesNothing()
    {
        var sut = ProductInwinningFilter.None;

        sut.ExcludesAnything.Should().BeFalse();
        sut.ExcludesRoadSegment(1).Should().BeFalse();
        sut.ExcludesRoadNode(1).Should().BeFalse();
        sut.ExcludesGradeSeparatedJunction(1, 2).Should().BeFalse();
    }

    [Fact]
    public void ExcludesTheGivenRoadSegmentsAndRoadNodes()
    {
        var sut = new ProductInwinningFilter([1, 2], [10]);

        sut.ExcludesAnything.Should().BeTrue();
        sut.ExcludesRoadSegment(1).Should().BeTrue();
        sut.ExcludesRoadSegment(3).Should().BeFalse();
        sut.ExcludesRoadNode(10).Should().BeTrue();
        sut.ExcludesRoadNode(1).Should().BeFalse();
    }

    [Fact]
    public void ExcludesAnythingWhenItExcludesOnlyRoadNodes()
    {
        var sut = new ProductInwinningFilter([], [10]);

        sut.ExcludesAnything.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(3, 1, true)]
    [InlineData(1, 2, true)]
    [InlineData(3, 4, false)]
    public void ExcludesAGradeSeparatedJunctionWhenEitherOfItsRoadSegmentsIsExcluded(int upperRoadSegmentId, int lowerRoadSegmentId, bool excluded)
    {
        var sut = new ProductInwinningFilter([1, 2], []);

        sut.ExcludesGradeSeparatedJunction(upperRoadSegmentId, lowerRoadSegmentId).Should().Be(excluded);
    }

    [Fact]
    // A road segment that was once part of a completed inwinning counts as completed, also when a later inwinning locks
    // it again.
    public async Task RoadSegmentsWithAtLeastOneCompletedInwinningAreCompleted()
    {
        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        extractsDbContext.InwinningRoadSegments.AddRange(
            new InwinningRoadSegment { RoadSegmentId = 1, NisCode = "11001", Completed = true },
            new InwinningRoadSegment { RoadSegmentId = 2, NisCode = "11001", Completed = true },
            new InwinningRoadSegment { RoadSegmentId = 2, NisCode = "11002", Completed = false },
            new InwinningRoadSegment { RoadSegmentId = 3, NisCode = "11002", Completed = false },
            new InwinningRoadSegment { RoadSegmentId = 4, NisCode = null, Completed = true });
        await extractsDbContext.SaveChangesAsync();

        var roadSegmentIds = await extractsDbContext.GetAtLeastOnceCompletedInwinningRoadSegmentIds(CancellationToken.None);

        roadSegmentIds.Select(x => x.ToInt32()).Should().BeEquivalentTo([1, 2, 4]);
    }

    [Fact]
    public async Task CreateExcludesTheRoadNodesTheCompletedRoadSegmentsStartOrEndAt()
    {
        await using var context = CreateProductContext();
        context.RoadSegments.AddRange(
            RoadSegment(id: 1, startNodeId: 10, endNodeId: 11),
            RoadSegment(id: 2, startNodeId: 11, endNodeId: 12),
            RoadSegment(id: 3, startNodeId: 12, endNodeId: 13));
        await context.SaveChangesAsync();

        var sut = await ProductInwinningFilter.CreateAsync([1, 3, 99], context, CancellationToken.None);

        sut.ExcludesAnything.Should().BeTrue();
        new[] { 1, 2, 3, 99 }.Where(sut.ExcludesRoadSegment).Should().BeEquivalentTo([1, 3, 99]);
        new[] { 10, 11, 12, 13 }.Where(sut.ExcludesRoadNode).Should().BeEquivalentTo([10, 11, 12, 13]);
    }

    [Fact]
    public async Task CreateLeavesTheRoadNodesOfOtherRoadSegments()
    {
        await using var context = CreateProductContext();
        context.RoadSegments.AddRange(
            RoadSegment(id: 1, startNodeId: 10, endNodeId: 11),
            RoadSegment(id: 2, startNodeId: 12, endNodeId: 13));
        await context.SaveChangesAsync();

        var sut = await ProductInwinningFilter.CreateAsync([1], context, CancellationToken.None);

        new[] { 10, 11, 12, 13 }.Where(sut.ExcludesRoadNode).Should().BeEquivalentTo([10, 11]);
    }

    [Fact]
    public async Task CreateWithoutCompletedRoadSegmentsIsNone()
    {
        await using var context = CreateProductContext();

        var sut = await ProductInwinningFilter.CreateAsync([], context, CancellationToken.None);

        sut.Should().BeSameAs(ProductInwinningFilter.None);
    }

    [Theory]
    [InlineData(new int[0], 4)]
    [InlineData(new[] { 1 }, 2)]
    [InlineData(new[] { 1, 2 }, 1)]
    [InlineData(new[] { 99 }, 4)]
    public async Task CountsTheRecordsOfRoadSegmentsThatAreNotExcluded(int[] excludedRoadSegmentIds, int expectedCount)
    {
        await using var context = CreateProductContext();
        context.RoadSegmentLaneAttributes.AddRange(
            new RoadSegmentLaneAttributeRecord { Id = 1, RoadSegmentId = 1, DbaseRecord = [] },
            new RoadSegmentLaneAttributeRecord { Id = 2, RoadSegmentId = 1, DbaseRecord = [] },
            new RoadSegmentLaneAttributeRecord { Id = 3, RoadSegmentId = 2, DbaseRecord = [] },
            new RoadSegmentLaneAttributeRecord { Id = 4, RoadSegmentId = 3, DbaseRecord = [] });
        await context.SaveChangesAsync();

        var sut = new ProductInwinningFilter(excludedRoadSegmentIds, []);

        var count = await sut.CountRoadSegmentRecordsAsync(context.RoadSegmentLaneAttributes.Select(x => x.RoadSegmentId), CancellationToken.None);

        count.Should().Be(expectedCount);
    }

    private static ProductContext CreateProductContext()
    {
        return new ProductContext(new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);
    }

    private static RoadSegmentRecord RoadSegment(int id, int startNodeId, int endNodeId)
    {
        return new RoadSegmentRecord
        {
            Id = id,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            ShapeRecordContent = [],
            DbaseRecord = []
        };
    }
}
