namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName.WhenUnlinking;

using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using BackOffice.Extensions;
using BackOffice.Framework;
using BackOffice.Handlers.RoadSegments;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using Handlers;
using Hosts;
using Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Requests;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class UnlinkStreetNameRequestHandlerTests : LinkUnlinkStreetNameTestsBase
{
    public UnlinkStreetNameRequestHandlerTests(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
    }

    private async Task HandleRequest(ITicketing ticketing, UnlinkStreetNameRequest request)
    {
        var idempotentCommandHandler = new RoadRegistryIdempotentCommandHandler(Container.Resolve<CommandHandlerDispatcher>());

        var handler = new UnlinkStreetNameSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            ticketing,
            idempotentCommandHandler,
            RoadRegistryContext,
            new ChangeRoadNetworkDispatcher(
                new RoadNetworkCommandQueue(Store, ApplicationMetadata),
                idempotentCommandHandler,
                EntityMapFactory.Resolve<EventSourcedEntityMap>(),
                new FakeOrganizationRepository(),
                LoggerFactory.CreateLogger<ChangeRoadNetworkDispatcher>()),
            new FakeDistributedStreamStoreLockOptions(),
            new UseDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle(false),
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
    public async Task UnlinkStreetNameFromRoadSegment_LeftAndRightStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;
        TestData.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(WellKnownStreetNameIds.Proposed), StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        var roadSegmentModified = command!.Changes.Single().RoadSegmentModified;

        Xunit.Assert.NotEmpty(roadSegmentModified.Lanes);
        Xunit.Assert.NotEmpty(roadSegmentModified.Surfaces);
        Xunit.Assert.NotEmpty(roadSegmentModified.Widths);
        Xunit.Assert.Equal(CrabStreetNameId.NotApplicable, roadSegmentModified.LeftSide.StreetNameId);
        Xunit.Assert.Equal(CrabStreetNameId.NotApplicable, roadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public void UnlinkStreetNameFromRoadSegment_LeftOrRightStreetName_IsRequired()
    {
        var validator = new UnlinkStreetNameRequestValidator();

        var validationResult = validator.Validate(new UnlinkStreetNameRequest(new RoadSegmentId(1), RoadSegmentGeometryDrawMethod.Measured, null, null));

        var error = validationResult.Errors.TranslateToDutch().Single();
        Xunit.Assert.Equal("request", error.PropertyName);
        Xunit.Assert.Equal("JsonInvalid", error.ErrorCode);
        Xunit.Assert.Equal("Ongeldig JSON formaat.", error.ErrorMessage);
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        var linkerstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Proposed);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, linkerstraatnaamPuri, null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_NotLinkedToTheOneBeingUnlinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        var linkerstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Current);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, linkerstraatnaamPuri, null));

        //Assert
        VerifyThatTicketHasError(ticketing, "LinkerstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_LeftStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(WellKnownStreetNameIds.Proposed), null));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(CrabStreetNameId.NotApplicable, command!.Changes.Single().RoadSegmentModified.LeftSide.StreetNameId);
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        //Act
        var rechterstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Proposed);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, rechterstraatnaamPuri));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_NotLinkedToTheOneBeingUnlinked()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = TestData.Segment1Added.Id;

        TestData.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        var rechterstraatnaamPuri = StreetNamePuri(WellKnownStreetNameIds.Current);
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, rechterstraatnaamPuri));

        //Assert
        VerifyThatTicketHasError(ticketing, "RechterstraatnaamNietGekoppeld", $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'");
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RightStreetName_Succeeded()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        TestData.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, StreetNamePuri(WellKnownStreetNameIds.Proposed)));

        //Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        VerifyThatTicketHasCompleted(ticketing, string.Format(Options.DetailUrl, roadSegmentId), roadSegment.LastEventHash);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        Xunit.Assert.Equal(CrabStreetNameId.NotApplicable, command!.Changes.Single().RoadSegmentModified.RightSide.StreetNameId);
    }

    [Fact]
    public async Task UnlinkStreetNameFromRoadSegment_RoadSegment_NotExists()
    {
        //Arrange
        var ticketing = new Mock<ITicketing>();
        var roadSegmentId = int.MaxValue;

        await GivenSegment1Added();

        //Act
        await HandleRequest(ticketing.Object, new UnlinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, StreetNamePuri(99999), null));

        //Assert
        VerifyThatTicketHasError(ticketing, "NotFound", $"Het wegsegment met id {roadSegmentId} bestaat niet.");
    }

    [Fact]
    public Task WhenProcessing_UnlinkStreetNameSqsRequest_Then_UnlinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<UnlinkStreetNameSqsRequest, UnlinkStreetNameSqsLambdaRequest, UnlinkStreetNameRequest>();
    }

    private new static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }
}
