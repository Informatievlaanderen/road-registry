namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Inwinning;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections;

public partial class InwinningsstatusControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("12345")]
    public async Task GivenCompletedInwinningRoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenCompleet(string nisCode)
    {
        // Arrange
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            NisCode = nisCode,
            Completed = true
        });
        await _extractsDbContext.SaveChangesAsync();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Compleet);
    }

    [Fact]
    public async Task GivenNotCompletedInwinningRoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenLocked()
    {
        // Arrange
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            NisCode = null,
            Completed = false
        });
        await _extractsDbContext.SaveChangesAsync();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Locked);
    }

    [Fact]
    public async Task GivenNotCompletedAndCompletedInwinningRoadSegments_WhenGettingWegsegmentInwinningsstatus_ThenLocked()
    {
        // Arrange
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            NisCode = "12345",
            Completed = true
        });
        _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            NisCode = null,
            Completed = false
        });
        await _extractsDbContext.SaveChangesAsync();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Locked);
    }

    [Fact]
    public async Task GivenNoInwinningRoadSegmentButExistingRoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenNietGestart()
    {
        // Arrange
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        await using(var session = store.LightweightSession())
        {
            var extractItem = Fixture.Create<RoadSegmentExtractItem>();
            extractItem.RoadSegmentId = roadSegmentId;

            session.Store(extractItem);
        }

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.NietGestart);
    }

    [Fact]
    public async Task GivenNoInwinningRoadSegmentButV2RoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenCompleet()
    {
        // Arrange: a V2 road segment that takes part in no inwinning - one of the parts of a split, for instance
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        await using (var session = store.LightweightSession())
        {
            session.Store(BuildReadItem(roadSegmentId, isV2: true));
        }

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Compleet);
    }

    [Fact]
    public async Task GivenNotCompletedInwinningRoadSegmentForV2RoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenLocked()
    {
        // Arrange: a V2 road segment that a new inwinning has taken up again
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            NisCode = "12345",
            Completed = false
        });
        await _extractsDbContext.SaveChangesAsync();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        await using (var session = store.LightweightSession())
        {
            session.Store(BuildReadItem(roadSegmentId, isV2: true));
        }

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<WegsegmentInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Locked);
    }

    private RoadSegmentReadItem BuildReadItem(RoadSegmentId roadSegmentId, bool isV2)
    {
        var readItem = Fixture.Create<RoadSegmentReadItem>();
        readItem.RoadSegmentId = roadSegmentId;
        readItem.IsV2 = isV2;
        return readItem;
    }

    [Fact]
    public async Task GivenNoInwinningRoadSegmentAndNoExtractRoadSegment_WhenGettingWegsegmentInwinningsstatus_ThenNotFound()
    {
        // Arrange
        var roadSegmentId = Fixture.Create<RoadSegmentId>();

        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        // Act
        var result = await Controller.GetWegsegmentInwinningsstatus(
            roadSegmentId,
            _extractsDbContext,
            store,
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }
}
