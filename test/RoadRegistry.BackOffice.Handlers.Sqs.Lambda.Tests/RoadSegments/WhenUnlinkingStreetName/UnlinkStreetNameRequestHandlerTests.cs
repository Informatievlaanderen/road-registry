namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenUnlinkingStreetName;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Dbase;
using Framework;
using Handlers;
using Microsoft.Extensions.Configuration;
using Moq;
using Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Framework;
using Common;
using Messages;
using Sqs.RoadSegments;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class UnlinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public UnlinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    private async Task HandleRequest(ITicketing ticketing, UnlinkStreetNameRequest request)
    {
        var handler = new UnlinkStreetNameSqsLambdaRequestHandler(
            Container.Resolve<IConfiguration>(),
            new FakeRetryPolicy(),
            ticketing,
            new RoadRegistryIdempotentCommandHandler(Container.Resolve<CommandHandlerDispatcher>()),
            RoadRegistryContext,
            new RoadNetworkCommandQueue(Store, ApplicationMetadata)
        );

        await handler.Handle(new UnlinkStreetNameSqsLambdaRequest(RoadNetworkInfo.Identifier.ToString(), new UnlinkStreetNameSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = Fixture.Create<ProvenanceData>()
        }), CancellationToken.None);
    }
    
    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        var linkerstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Proposed);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, linkerstraatnaamPuri, null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinkedToTheOneBeingUnlinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        var linkerstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Current);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, linkerstraatnaamPuri, null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(Segment1Added.Id);

        Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, StreetNamePuri(WellKnownStreetNameIds.Proposed), null));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get();
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(ConfigDetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(0, command!.Changes.Single().RoadSegmentModified.LeftSide.StreetNameId);
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        var rechterstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Proposed);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, null, rechterstraatnaamPuri));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinkedToTheOneBeingUnlinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        var rechterstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Current);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, null, rechterstraatnaamPuri));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(Segment1Added.Id);

        Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, null, StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get();
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(ConfigDetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(0, command!.Changes.Single().RoadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RoadSegment_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = int.MaxValue;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "NotFound", "Onbestaand wegsegment.");
    }

    private static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }
}


