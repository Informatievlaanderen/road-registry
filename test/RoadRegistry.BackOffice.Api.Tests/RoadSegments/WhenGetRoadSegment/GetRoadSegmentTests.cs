namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenGetRoadSegment;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Api.Infrastructure.Controllers;
using Api.Infrastructure.Options;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class GetRoadSegmentTests : IAsyncLifetime
{
    private const string BaseUrl = "https://api.basisregisters.vlaanderen.be";

    private readonly DbContextBuilder _dbContextBuilder;
    private readonly IFixture _fixture;
    private readonly Mock<IMediator> _mediator = new();
    private ExtractsDbContext _extractsDbContext;

    public GetRoadSegmentTests(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _fixture = new RoadNetworkTestData().ObjectProvider;

        _mediator
            .Setup(x => x.Send(It.IsAny<RoadSegmentDetailRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RoadSegmentNotFoundException());
    }

    public Task InitializeAsync()
    {
        _extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _extractsDbContext.DisposeAsync();
    }

    [Fact]
    public async Task GivenCompletedInwinning_ThenNotFoundWithLinkToV3()
    {
        var roadSegmentId = _fixture.Create<RoadSegmentId>();
        await GivenInwinningRoadSegments((roadSegmentId, true));

        var (result, httpContext) = await GetRoadSegment(roadSegmentId);

        result.Should().BeOfType<NotFoundResult>();
        httpContext.Response.Headers.Link.ToString().Should().Be($"<{BaseUrl}/v3/wegsegmenten/{roadSegmentId}>; rel=\"successor-version\"");
        _mediator.Verify(x => x.Send(It.IsAny<RoadSegmentDetailRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenInwinningOfWhichOneIsNotCompleted_ThenLookedUp()
    {
        var roadSegmentId = _fixture.Create<RoadSegmentId>();
        await GivenInwinningRoadSegments((roadSegmentId, true), (roadSegmentId, false));

        var (result, httpContext) = await GetRoadSegment(roadSegmentId);

        result.Should().BeOfType<NotFoundResult>();
        httpContext.Response.Headers.Link.Should().BeEmpty();
        _mediator.Verify(x => x.Send(It.Is<RoadSegmentDetailRequest>(request => request.WegsegmentId == roadSegmentId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenNoInwinning_ThenLookedUp()
    {
        var roadSegmentId = _fixture.Create<RoadSegmentId>();

        var (result, httpContext) = await GetRoadSegment(roadSegmentId);

        result.Should().BeOfType<NotFoundResult>();
        httpContext.Response.Headers.Link.Should().BeEmpty();
        _mediator.Verify(x => x.Send(It.Is<RoadSegmentDetailRequest>(request => request.WegsegmentId == roadSegmentId), It.IsAny<CancellationToken>()), Times.Once);
    }

    private async Task GivenInwinningRoadSegments(params (RoadSegmentId RoadSegmentId, bool Completed)[] inwinningRoadSegments)
    {
        foreach (var (roadSegmentId, completed) in inwinningRoadSegments)
        {
            _extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
            {
                RoadSegmentId = roadSegmentId,
                NisCode = _fixture.Create<string>()[..5],
                Completed = completed
            });
        }

        await _extractsDbContext.SaveChangesAsync();
    }

    private async Task<(IActionResult Result, HttpContext HttpContext)> GetRoadSegment(RoadSegmentId roadSegmentId)
    {
        var httpContext = new DefaultHttpContext();
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor { HttpContext = httpContext }), _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.GetRoadSegment(roadSegmentId, _extractsDbContext, new ApiOptions { BaseUrl = BaseUrl }, CancellationToken.None);

        return (result, httpContext);
    }
}
