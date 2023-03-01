namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName.WhenUnlinking;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using Hosts;
using Microsoft.Extensions.Configuration;
using Moq;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;

public class UnlinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public UnlinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
    }

    private async Task HandleRequest(ITicketing ticketing, UnlinkStreetNameRequest request)
    {
        var handler = new UnlinkStreetNameSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            ticketing,
            new RoadRegistryIdempotentCommandHandler(Container.Resolve<CommandHandlerDispatcher>()),
            RoadRegistryContext,
            new RoadNetworkCommandQueue(Store, ApplicationMetadata),
            new FakeDistributedStreamStoreLockOptions(),
            LoggerFactory.CreateLogger<UnlinkStreetNameSqsLambdaRequestHandler>()
        );

        await handler.Handle(new UnlinkStreetNameSqsLambdaRequest(RoadNetwork.Identifier.ToString(), new UnlinkStreetNameSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        }), CancellationToken.None);
    }

    [Fact]
    public Task WhenProcessing_UnlinkStreetNameSqsRequest_Then_UnlinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<UnlinkStreetNameSqsRequest, UnlinkStreetNameSqsLambdaRequest, UnlinkStreetNameRequest>();
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
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(CrabStreetnameId.NotApplicable, command!.Changes.Single().RoadSegmentModified.LeftSide.StreetNameId);
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
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(CrabStreetnameId.NotApplicable, command!.Changes.Single().RoadSegmentModified.RightSide.StreetNameId);
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

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftAndRightStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(Segment1Added.Id);

        Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;
        Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, StreetNamePuri(WellKnownStreetNameIds.Proposed), StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        var roadSegmentModified = command!.Changes.Single().RoadSegmentModified;
        Xunit.Assert.Equal(CrabStreetnameId.NotApplicable, roadSegmentModified.LeftSide.StreetNameId);
        Xunit.Assert.Equal(CrabStreetnameId.NotApplicable, roadSegmentModified.RightSide.StreetNameId);
    }

    private new static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }
}


