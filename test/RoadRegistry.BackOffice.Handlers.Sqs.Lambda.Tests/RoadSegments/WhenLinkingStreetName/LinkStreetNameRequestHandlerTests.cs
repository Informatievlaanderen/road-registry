namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenLinkingStreetName;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
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

public class LinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public LinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }
    
    private async Task HandleRequest(ITicketing ticketing, LinkStreetNameRequest request)
    {
        var handler = new LinkStreetNameSqsLambdaRequestHandler(
            Container.Resolve<IConfiguration>(),
            new FakeRetryPolicy(),
            ticketing,
            new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), IdempotencyContext),
            RoadRegistryContext,
            StreetNameCache,
            new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.Lambda))
        );

        await handler.Handle(new LinkStreetNameSqsLambdaRequest(RoadNetworkInfo.Identifier.ToString(), new LinkStreetNameSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = Fixture.Create<ProvenanceData>()
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
        //TODO-rik fix success test, then set up the same for unlink
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get();
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(ConfigDetailUrl, roadSegmentId), roadSegment.LastEventHash);

        //var command = await Store.GetLastCommand<ChangeRoadNetwork>();

        //Xunit.Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.LeftSideStreetNameId);
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

    //[InlineData(WellKnownStreetNameIds.Proposed)]
    //[InlineData(WellKnownStreetNameIds.Current)]
    //[Theory]
    //public async Task LinkStreetNameToRoadSegment_RightStreetName_Proposed_Current(int streetNameId)
    //{
    //    Fixture.Segment1Added.RightSide.StreetNameId = null;

    //    await GivenSegment1Added();

    //    var request = new LinkStreetNameRequest(1, null, StreetNamePuri(streetNameId));
    //    await Handler.HandleAsync(request, CancellationToken.None);

    //    var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

    //    Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.RightSideStreetNameId);
    //}

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
