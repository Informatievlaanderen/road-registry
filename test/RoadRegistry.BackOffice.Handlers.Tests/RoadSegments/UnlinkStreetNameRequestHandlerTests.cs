//namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments;

//using Abstractions.Exceptions;
//using Abstractions.RoadSegments;
//using BackOffice.Framework;
//using Common;
//using FluentValidation;
//using Handlers.RoadSegments;
//using Messages;
//using Microsoft.Extensions.Logging;

//public class UnlinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
//    <UnlinkStreetNameRequest, UnlinkStreetNameResponse, UnlinkStreetNameRequestHandler>
//{
//    public UnlinkStreetNameRequestHandlerTests(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<UnlinkStreetNameRequestHandler> logger)
//        : base(commandHandlerDispatcher, logger)
//    {
//    }

//    protected override UnlinkStreetNameRequestHandler CreateHandler(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<UnlinkStreetNameRequestHandler> logger)
//    {
//        return new UnlinkStreetNameRequestHandler(commandHandlerDispatcher, logger, Fixture.Store, RoadRegistryContext);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinked()
//    {
//        Fixture.Segment1Added.LeftSide.StreetNameId = null;

//        await GivenSegment1Added();

//        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
//        {
//            var request = new UnlinkStreetNameRequest(1, StreetNamePuri(WellKnownStreetNameIds.Proposed), null);
//            return Handler.HandleAsync(request, CancellationToken.None);
//        });
//        Assert.Equal("LinkerstraatnaamNietGekoppeld", ex.Errors.Single().ErrorCode);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinkedToTheOneBeingUnlinked()
//    {
//        Fixture.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

//        await GivenSegment1Added();

//        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
//        {
//            var request = new UnlinkStreetNameRequest(1, StreetNamePuri(WellKnownStreetNameIds.Current), null);
//            return Handler.HandleAsync(request, CancellationToken.None);
//        });
//        Assert.Equal("LinkerstraatnaamNietGekoppeld", ex.Errors.Single().ErrorCode);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_Succeeded()
//    {
//        Fixture.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

//        await GivenSegment1Added();

//        var request = new UnlinkStreetNameRequest(1, StreetNamePuri(WellKnownStreetNameIds.Proposed), null);
//        await Handler.HandleAsync(request, CancellationToken.None);

//        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

//        Assert.Equal(0, command!.Changes.Single().ModifyRoadSegment.LeftSideStreetNameId);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinked()
//    {
//        Fixture.Segment1Added.RightSide.StreetNameId = null;

//        await GivenSegment1Added();

//        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
//        {
//            var request = new UnlinkStreetNameRequest(1, null, StreetNamePuri(WellKnownStreetNameIds.Proposed));
//            return Handler.HandleAsync(request, CancellationToken.None);
//        });
//        Assert.Equal("RechterstraatnaamNietGekoppeld", ex.Errors.Single().ErrorCode);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinkedToTheOneBeingUnlinked()
//    {
//        Fixture.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

//        await GivenSegment1Added();

//        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
//        {
//            var request = new UnlinkStreetNameRequest(1, null, StreetNamePuri(WellKnownStreetNameIds.Current));
//            return Handler.HandleAsync(request, CancellationToken.None);
//        });
//        Assert.Equal("RechterstraatnaamNietGekoppeld", ex.Errors.Single().ErrorCode);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_Succeeded()
//    {
//        Fixture.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

//        await GivenSegment1Added();

//        var request = new UnlinkStreetNameRequest(1, null, StreetNamePuri(WellKnownStreetNameIds.Proposed));
//        await Handler.HandleAsync(request, CancellationToken.None);

//        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

//        Assert.Equal(0, command!.Changes.Single().ModifyRoadSegment.RightSideStreetNameId);
//    }

//    [Fact]
//    public async Task UnlinkStreetNameFromRoadSegment_RoadSegment_NotExists()
//    {
//        await GivenSegment1Added();

//        await Assert.ThrowsAsync<RoadSegmentNotFoundException>(() =>
//        {
//            var request = new UnlinkStreetNameRequest(int.MaxValue, StreetNamePuri(99999), null);
//            return Handler.HandleAsync(request, CancellationToken.None);
//        });
//    }

//    private static class WellKnownStreetNameIds
//    {
//        public const int Proposed = 1;
//        public const int Current = 2;
//        public const int Retired = 3;
//    }
//}
