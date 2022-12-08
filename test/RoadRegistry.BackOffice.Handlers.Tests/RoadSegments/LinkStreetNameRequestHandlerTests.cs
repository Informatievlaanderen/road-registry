namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Framework;
using Common;
using FluentValidation;
using Handlers.RoadSegments;
using Messages;
using Microsoft.Extensions.Logging;

public class LinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
    <LinkStreetNameRequest, LinkStreetNameResponse, LinkStreetNameRequestHandler>
{
    public LinkStreetNameRequestHandlerTests(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<LinkStreetNameRequestHandler> logger)
        : base(commandHandlerDispatcher, logger)
    {
    }

    protected override LinkStreetNameRequestHandler CreateHandler(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<LinkStreetNameRequestHandler> logger)
    {
        return new LinkStreetNameRequestHandler(commandHandlerDispatcher, logger, Fixture.Store, RoadRegistryContext, StreetNameCache);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_AlreadyLinked()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, StreetNamePuri(WellKnownStreetNameIds.Proposed), null);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("LinkerstraatnaamNietOntkoppeld", ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_NotExists()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, StreetNamePuri(99999), null);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("StraatnaamNietGekend", ex.Errors.Single().ErrorCode);
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Proposed_Current(int streetNameId)
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var request = new LinkStreetNameRequest(1, StreetNamePuri(streetNameId), null);
        await Handler.HandleAsync(request, CancellationToken.None);

        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

        Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.LeftSideStreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Retired()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, StreetNamePuri(WellKnownStreetNameIds.Retired), null);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_AlreadyLinked()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, null, StreetNamePuri(WellKnownStreetNameIds.Proposed));
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("RechterstraatnaamNietOntkoppeld", ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_NotExists()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, null, StreetNamePuri(99999));
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("StraatnaamNietGekend", ex.Errors.Single().ErrorCode);
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Proposed_Current(int streetNameId)
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var request = new LinkStreetNameRequest(1, null, StreetNamePuri(streetNameId));
        await Handler.HandleAsync(request, CancellationToken.None);

        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

        Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.RightSideStreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Retired()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkStreetNameRequest(1, null, StreetNamePuri(WellKnownStreetNameIds.Retired));
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RoadSegment_NotExists()
    {
        await GivenSegment1Added();

        await Assert.ThrowsAsync<RoadSegmentNotFoundException>(() =>
        {
            var request = new LinkStreetNameRequest(int.MaxValue, StreetNamePuri(99999), null);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
    }

    
}
