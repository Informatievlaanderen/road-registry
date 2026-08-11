namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangingOutlineGeometry;

using Abstractions.RoadSegmentsOutline;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
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
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using Xunit.Abstractions;
using GeometryTranslator = GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public class GivenOrganizationExists : BackOfficeLambdaTest
{
    public GivenOrganizationExists(ITestOutputHelper outputHelper) : base(outputHelper)
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
            GeometryConfiguration.GeometryFactory)
            .ToMultiLineString();

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(lineString)
        );

        // Act
        var translatedChanges = await HandleRequest(request);

        // Assert
        await VerifyThatTicketHasCompleted(roadSegmentId);

        translatedChanges.Should().HaveCount(1);

        var modifyRoadSegmentGeometry = Xunit.Assert.IsType<ModifyRoadSegment>(translatedChanges[0]);
        modifyRoadSegmentGeometry.Id.Should().Be(roadSegmentId);
        Assert.Equal(lineString, modifyRoadSegmentGeometry.Geometry);
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
        VerifyThatTicketHasError(
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
        VerifyThatTicketHasError(
            "MiddellijnGeometrieKorterDanMinimum",
            "De opgegeven geometrie van wegsegment met id 1 heeft niet de minimale lengte van 2 meter.");
    }

    [Fact]
    public async Task GivenInwinningRoadSegment_ThenError()
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var lineString = new LineString(
                new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(2, 0, 2)]),
                GeometryConfiguration.GeometryFactory)
            .ToMultiLineString();

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(lineString)
        );

        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            Completed = ObjectProvider.Create<bool>()
        });
        await extractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request, extractsDbContext: extractsDbContext);

        // Assert
        VerifyThatTicketHasError("RoadSegmentIsInInwinning", $"Het wegsegment met id {roadSegmentId} heeft de inwinningsstatus 'locked' of 'compleet'.");
    }

    // A v1 outline geometry arrives in Lambert 72 while the inwinningszone contours are held in Lambert 2008.
    // Intersects compares raw coordinates and never looks at the SRID, so unless the road segment is put in the
    // zone's reference system first the two read as a hundred kilometres apart and every road passes the check.
    private const double Lambert08X = 217368.75;
    private const double Lambert08Y = 181577.02;

    private static MultiLineString Lambert72LineAround(double lambert08X, double lambert08Y)
    {
        var line = new MultiLineString([
            new LineString([new Coordinate(lambert08X - 50, lambert08Y), new Coordinate(lambert08X + 50, lambert08Y)])
        ]).WithSrid(WellknownSrids.Lambert08);

        return line.TransformFromLambert08To72().WithSrid(WellknownSrids.Lambert72);
    }

    private static ExtractsDbContext ExtractsDbContextWithZoneAround(double lambert08X, double lambert08Y)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        db.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = "11001",
            Operator = "op",
            DownloadId = Guid.NewGuid(),
            Contour = new Polygon(new LinearRing([
                new Coordinate(lambert08X - 500, lambert08Y - 500),
                new Coordinate(lambert08X + 500, lambert08Y - 500),
                new Coordinate(lambert08X + 500, lambert08Y + 500),
                new Coordinate(lambert08X - 500, lambert08Y + 500),
                new Coordinate(lambert08X - 500, lambert08Y - 500)
            ])).WithSrid(WellknownSrids.Lambert08),
            Completed = false
        });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task GivenGeometryOverlappingAnInwinningszone_ThenError()
    {
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(Lambert72LineAround(Lambert08X, Lambert08Y)));

        await HandleRequest(request, extractsDbContext: ExtractsDbContextWithZoneAround(Lambert08X, Lambert08Y));

        VerifyThatTicketHasError("RoadSegmentOverlapsWithInwinningszone",
            $"Het wegsegment met id {roadSegmentId} valt (gedeeltelijk) binnen een gemeente die de inwinningsstatus 'locked' of 'compleet' heeft.");
    }

    [Fact]
    public async Task GivenGeometryOutsideEveryInwinningszone_ThenSucceeded()
    {
        // The same conversion is applied, but the road genuinely lies elsewhere: it must not be refused.
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(Lambert72LineAround(Lambert08X + 5000, Lambert08Y)));

        var translatedChanges = await HandleRequest(request, extractsDbContext: ExtractsDbContextWithZoneAround(Lambert08X, Lambert08Y));

        translatedChanges.OfType<ModifyRoadSegment>().Should().ContainSingle();
    }

    private async Task<IReadOnlyList<ITranslatedChange>> HandleRequest(
        ChangeRoadSegmentOutlineGeometryRequest request,
        ExtractsDbContext? extractsDbContext = null)
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
            extractsDbContext ?? new FakeExtractsDbContextFactory().CreateDbContext(),
            new NullLogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler>()
        );

        await handler.Handle(sqsLambdaRequest, CancellationToken.None);

        return translatedChanges.ToList();
    }
}
