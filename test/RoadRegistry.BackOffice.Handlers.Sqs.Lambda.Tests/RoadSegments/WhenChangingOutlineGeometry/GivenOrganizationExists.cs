namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangingOutlineGeometry;

using Abstractions.RoadSegmentsOutline;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using FluentAssertions;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Requests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using GeometryTranslator = GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadSegmentGeometry = BackOffice.Uploads.ModifyRoadSegmentGeometry;

public class GivenOrganizationExists : BackOfficeLambdaTest
{
    public GivenOrganizationExists()
    {
        ObjectProvider.CustomizeRoadSegmentOutlineGeometryDrawMethod();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(99999.99)]
    public async Task WhenValidRequest_ThenSucceeded(double length)
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var lineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(length, 0, length)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(lineString.ToMultiLineString())
        );

        // Act
        var translatedChanges = await HandleRequest(request);

        // Assert
        await VerifyThatTicketHasCompleted(roadSegmentId);

        translatedChanges.Should().HaveCount(1);

        var modifyRoadSegmentGeometry = Xunit.Assert.IsType<ModifyRoadSegmentGeometry>(translatedChanges[0]);
        modifyRoadSegmentGeometry.Id.Should().Be(roadSegmentId);
    }

    [Fact]
    public async Task WhenGeometryIsTooLong_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var tooLongLineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(100000, 0, 100000)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(tooLongLineString.ToMultiLineString())
        );

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasErrorList(
            "MiddellijnGeometrieTeLang",
            "De opgegeven geometrie van wegsegment met id 1 zijn lengte is groter of gelijk dan 100000 meter.");
    }

    [Fact]
    public async Task WhenGeometryIsTooShort_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var tooLongLineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(1.99, 0, 1.99)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(tooLongLineString.ToMultiLineString())
        );

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasErrorList(
            "MiddellijnGeometrieKorterDanMinimum",
            "De opgegeven geometrie van wegsegment met id 1 heeft niet de minimale lengte van 2 meter.");
    }

    private async Task<IReadOnlyList<ITranslatedChange>> HandleRequest(ChangeRoadSegmentOutlineGeometryRequest request)
    {
        var sqsRequest = new ChangeRoadSegmentOutlineGeometrySqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new ChangeRoadSegmentOutlineGeometrySqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        var translatedChanges = TranslatedChanges.Empty;
        var changeRoadNetworkDispatcherMock = new Mock<IChangeRoadNetworkDispatcher>();
        changeRoadNetworkDispatcherMock
            .Setup(x => x.DispatchAsync(
                It.IsAny<SqsLambdaRequest>(),
                It.IsAny<string>(),
                It.IsAny<Func<TranslatedChanges, Task<TranslatedChanges>>>(),
                It.IsAny<CancellationToken>()))
            .Callback(
                (SqsLambdaRequest _, string _, Func<TranslatedChanges, Task<TranslatedChanges>> builder, CancellationToken _) =>
                {
                    translatedChanges = builder(translatedChanges).GetAwaiter().GetResult();
                });

        var handler = new ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            changeRoadNetworkDispatcherMock.Object,
            new NullLogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler>()
        );

        await handler.Handle(sqsLambdaRequest, CancellationToken.None);

        return translatedChanges.ToList();
    }
}
