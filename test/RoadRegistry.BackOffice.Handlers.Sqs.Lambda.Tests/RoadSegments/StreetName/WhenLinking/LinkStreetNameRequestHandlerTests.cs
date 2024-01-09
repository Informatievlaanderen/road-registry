namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName.WhenLinking;

using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using BackOffice.Extensions;
using BackOffice.Framework;
using BackOffice.Handlers.RoadSegments;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using FeatureToggles;
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
    public LinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
    }

    private async Task HandleRequest(ITicketing ticketing, LinkStreetNameRequest request)
    {
        var idempotentCommandHandler = new RoadRegistryIdempotentCommandHandler(Container.Resolve<CommandHandlerDispatcher>());

        var handler = new LinkStreetNameSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            ticketing,
            idempotentCommandHandler,
            RoadRegistryContext,
            StreetNameClient,
            new ChangeRoadNetworkDispatcher(
                new RoadNetworkCommandQueue(Store, ApplicationMetadata),
                idempotentCommandHandler,
                EntityMapFactory.Resolve<EventSourcedEntityMap>(),
                new FakeOrganizationRepository(),
                LoggerFactory.CreateLogger<ChangeRoadNetworkDispatcher>()),
            new FakeDistributedStreamStoreLockOptions(),
            new UseDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle(false),
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(streetNameId), StreetNamePuri(streetNameId)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(WellKnownStreetNameIds.Proposed), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietOntkoppeld", $"Het wegsegment '{roadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.");
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(WellKnownStreetNameIds.Null), null));

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
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(streetNameId), null));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(WellKnownStreetNameIds.Retired), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietOntkoppeld", $"Het wegsegment '{roadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.");
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, StreetNamePuri(99999)));

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
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, StreetNamePuri(streetNameId)));

        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, StreetNamePuri(WellKnownStreetNameIds.Retired)));

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
        await HandleRequest(ticketing.Object, new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "NotFound", $"Het wegsegment met id {roadSegmentId} bestaat niet.");
    }

    [Fact]
    public Task WhenProcessing_LinkStreetNameSqsRequest_Then_LinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<LinkStreetNameSqsRequest, LinkStreetNameSqsLambdaRequest, LinkStreetNameRequest>();
    }
}
