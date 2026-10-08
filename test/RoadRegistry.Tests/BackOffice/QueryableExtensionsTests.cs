namespace RoadRegistry.Tests.BackOffice;

using FluentAssertions;
using RoadRegistry.BackOffice.Extensions;

public class QueryableExtensionsTests
{
    private sealed record Item(int Id);

    [Fact]
    public async Task ForEachBatchAsync_OrdersByTheGivenKey()
    {
        // Each page is its own query, so the batches only line up when the whole set is ordered first. The source is
        // deliberately out of order: if the ordering were dropped again, the batches would come back in this order.
        var source = new[] { new Item(5), new Item(1), new Item(4), new Item(2), new Item(3) }.AsQueryable();

        var batches = new List<ICollection<Item>>();
        await source.ForEachBatchAsync(x => x.Id, 2, batch =>
        {
            batches.Add(batch);
            return Task.CompletedTask;
        }, CancellationToken.None);

        batches.Select(batch => batch.Select(x => x.Id).ToArray())
            .Should().BeEquivalentTo(new[] { new[] { 1, 2 }, [3, 4], [5] }, options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 5)]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    [InlineData(12, 5)]
    public async Task ForEachBatchAsync_HandsEveryItemToTheActionExactlyOnce(int itemCount, int batchSize)
    {
        var source = Enumerable.Range(1, itemCount).Reverse().Select(id => new Item(id)).AsQueryable();

        var seen = new List<int>();
        await source.ForEachBatchAsync(x => x.Id, batchSize, batch =>
        {
            seen.AddRange(batch.Select(x => x.Id));
            return Task.CompletedTask;
        }, CancellationToken.None);

        seen.Should().Equal(Enumerable.Range(1, itemCount));
    }
}
