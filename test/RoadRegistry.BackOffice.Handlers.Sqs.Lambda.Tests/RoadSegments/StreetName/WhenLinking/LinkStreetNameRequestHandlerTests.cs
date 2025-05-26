namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName.WhenLinking;

using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using BackOffice.Extensions;
using BackOffice.Handlers.RoadSegments;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using Handlers;
using Hosts;
using Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Requests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class LinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public LinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    private async Task HandleRequest(ITicketing ticketing, LinkStreetNameRequest request)
    {
        var handler = new LinkStreetNameSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            ticketing,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            StreetNameClient,
            ScopedContainer.Resolve<IChangeRoadNetworkDispatcher>(),
            new FakeDistributedStreamStoreLockOptions(),
            LoggerFactory.CreateLogger<LinkStreetNameSqsLambdaRequestHandler>()
        );

        await handler.Handle(new LinkStreetNameSqsLambdaRequest(RoadNetwork.Identifier.ToString(), new LinkStreetNameSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        }), CancellationToken.None);
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_LeftAndRightStreetName_Proposed_Current(int streetNameId)
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.LeftSide.StreetNameId = null;
        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(streetNameId), GetStreetNameIdAsString(streetNameId)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(SqsLambdaHandlerOptions.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        var roadSegmentModified = command!.Changes.Single().RoadSegmentModified;

        Xunit.Assert.NotEmpty(roadSegmentModified.Lanes);
        Xunit.Assert.NotEmpty(roadSegmentModified.Surfaces);
        Xunit.Assert.NotEmpty(roadSegmentModified.Widths);
        Xunit.Assert.Equal(streetNameId, roadSegmentModified.LeftSide.StreetNameId);
        Xunit.Assert.Equal(streetNameId, roadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public void LinkStreetNameToRoadSegment_LeftOrRightStreetName_IsRequired()
    {
        var validator = new LinkStreetNameRequestValidator();

        var validationResult = validator.Validate(new LinkStreetNameRequest(new RoadSegmentId(1), RoadSegmentGeometryDrawMethod.Measured, null, null));

        var error = validationResult.Errors.TranslateToDutch().Single();
        Xunit.Assert.Equal("request", error.PropertyName);
        Xunit.Assert.Equal("JsonInvalid", error.ErrorCode);
        Xunit.Assert.Equal("Ongeldig JSON formaat.", error.ErrorMessage);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_AlreadyLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(WellKnownStreetNameIds.Proposed), null));

        //Assert
        ticketing.VerifyThatTicketHasError("LinkerstraatnaamNietOntkoppeld", $"Het wegsegment met id {roadSegmentId} heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(99999), null));

        //Assert
        ticketing.VerifyThatTicketHasError("StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_NotFound()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(WellKnownStreetNameIds.Null), null));

        //Assert
        ticketing.VerifyThatTicketHasError("StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Proposed_Current(int streetNameId)
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(streetNameId), null));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(SqsLambdaHandlerOptions.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(streetNameId, command!.Changes.Single().RoadSegmentModified.LeftSide.StreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_LeftStreetName_Retired()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(WellKnownStreetNameIds.Retired), null));

        //Assert
        ticketing.VerifyThatTicketHasError("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_AlreadyLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, GetStreetNameIdAsString(WellKnownStreetNameIds.Proposed)));

        //Assert
        ticketing.VerifyThatTicketHasError("RechterstraatnaamNietOntkoppeld", $"Het wegsegment met id {roadSegmentId} heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, GetStreetNameIdAsString(99999)));

        //Assert
        ticketing.VerifyThatTicketHasError("StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Proposed_Current(int streetNameId)
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, GetStreetNameIdAsString(streetNameId)));

        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(SqsLambdaHandlerOptions.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(streetNameId, command!.Changes.Single().RoadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RightStreetName_Retired()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, GetStreetNameIdAsString(WellKnownStreetNameIds.Retired)));

        //Assert
        ticketing.VerifyThatTicketHasError("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
    }

    [Fact]
    public async Task LinkStreetNameToRoadSegment_RoadSegment_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = int.MaxValue;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, GetStreetNameIdAsString(99999), null));

        //Assert
        ticketing.VerifyThatTicketHasError("NotFound", $"Het wegsegment met id {roadSegmentId} bestaat niet.");
    }

    [Fact]
    public Task WhenProcessing_LinkStreetNameSqsRequest_Then_LinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<LinkStreetNameSqsRequest, LinkStreetNameSqsLambdaRequest, LinkStreetNameRequest>();
    }
}
