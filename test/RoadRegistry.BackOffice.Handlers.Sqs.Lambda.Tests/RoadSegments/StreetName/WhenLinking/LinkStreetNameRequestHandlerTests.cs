namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName.WhenLinking;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
using Microsoft.Extensions.Logging;
using RoadRegistry.Dbase;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class LinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public LinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
    }

    private async Task HandleRequest(ITicketing ticketing, LinkStreetNameRequest request)
    {
        var handler = new LinkStreetNameSqsLambdaRequestHandler(
            Container.Resolve<IConfiguration>(),
            new FakeRetryPolicy(),
            ticketing,
            new RoadRegistryIdempotentCommandHandler(Container.Resolve<CommandHandlerDispatcher>()),
            RoadRegistryContext,
            StreetNameCache,
            new RoadNetworkCommandQueue(Store, ApplicationMetadata),
            LoggerFactory.CreateLogger<LinkStreetNameSqsLambdaRequestHandler>()
        );

        await handler.Handle(new LinkStreetNameSqsLambdaRequest(RoadNetworkInfo.Identifier.ToString(), new LinkStreetNameSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        }), CancellationToken.None);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_AlreadyLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, StreetNamePuri(WellKnownStreetNameIds.Proposed), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietOntkoppeld", $"Het wegsegment '{roadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Proposed_Current(int streetNameId)
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(Segment1Added.Id);

        Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, StreetNamePuri(streetNameId), null));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get();
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(ConfigDetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(streetNameId, command!.Changes.Single().RoadSegmentModified.LeftSide.StreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Retired()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, StreetNamePuri(WellKnownStreetNameIds.Retired), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_AlreadyLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, null, StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietOntkoppeld", $"Het wegsegment '{roadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, null, StreetNamePuri(99999)));

        //Assert
        VerifyThatTicketHasError(ticketing, "StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Proposed_Current(int streetNameId)
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(Segment1Added.Id);

        Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, null, StreetNamePuri(streetNameId)));

        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get();
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(ConfigDetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(streetNameId, command!.Changes.Single().RoadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Retired()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = Segment1Added.Id;

        Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, null, StreetNamePuri(WellKnownStreetNameIds.Retired)));

        //Assert
        VerifyThatTicketHasError(ticketing, "WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RoadSegment_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = int.MaxValue;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "NotFound", "Onbestaand wegsegment.");
    }
}
