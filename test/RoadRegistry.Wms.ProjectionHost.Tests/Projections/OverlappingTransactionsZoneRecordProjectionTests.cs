namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Wms.Projections;

public class OverlappingTransactionZonesRecordProjectionTests
{
    private readonly Fixture _fixture;

    public OverlappingTransactionZonesRecordProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGotRequested();
        _fixture.CustomizeRoadNetworkExtractGotRequestedV2();
        _fixture.CustomizeRoadNetworkExtractClosed();
    }

    //TODO-rik tests

    [Fact]
    public Task When_extract_got_requested()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequested>();
        var extract2 = _fixture.Create<RoadNetworkExtractGotRequested>();

        var data = _fixture
            .CreateMany<RoadNetworkExtractGotRequested>()
            .Select(requested =>
            {
                var expected = new OverlappingTransactionZonesRecord
                {
                    DownloadId = requested.DownloadId,
                    Description = requested.Description,
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        return new OverlappingTransactionZonesProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_got_requested_v2()
    {
        var data = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .Select(requested =>
            {
                var expected = new OverlappingTransactionZonesRecord
                {
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    DownloadId = requested.DownloadId,
                    Description = requested.Description
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        return new OverlappingTransactionZonesProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_closed()
    {
        var requestedData = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .ToList();

        var closedData = requestedData
            .Select(knownRequested =>
            {
                var @event = new RoadNetworkExtractClosed
                {
                    RequestId = knownRequested.RequestId,
                    ExternalRequestId = knownRequested.ExternalRequestId,
                    DownloadIds = new [] { knownRequested.DownloadId.ToString("N") },
                    DateRequested = DateTime.UtcNow,
                    Reason = _fixture.Create<RoadNetworkExtractCloseReason>(),
                    When = knownRequested.When
                };

                return new
                {
                    @event
                };
            }).ToList();

        return new OverlappingTransactionZonesProjection()
            .Scenario()
            .Given(requestedData)
            .Given(closedData.Select(d => d.@event))
            .Expect([]);
    }

    [Fact]
    public Task When_extract_upload_changes_got_accepted()
    {
        var requestedData = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .ToList();

        var changesAcceptedData = requestedData
            .Select(knownRequested =>
            {
                var @event = new RoadNetworkChangesAccepted
                {
                    RequestId = knownRequested.RequestId,
                    DownloadId = knownRequested.DownloadId,
                    When = knownRequested.When
                };

                return new
                {
                    @event
                };
            }).ToList();

        return new OverlappingTransactionZonesProjection()
            .Scenario()
            .Given(requestedData)
            .Given(changesAcceptedData.Select(d => d.@event))
            .Expect([]);
    }

    [Fact]
    public Task When_grb_extract_got_uploaded()
    {
        var requestedData = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .Select(requested =>
            {
                var expected = new OverlappingTransactionZonesRecord
                {
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    DownloadId = requested.DownloadId,
                    Description = requested.Description
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        var changesAcceptedData = requestedData
            .Select(knownRequested =>
            {
                var @event = new RoadNetworkExtractChangesArchiveUploaded
                {
                    RequestId = knownRequested.@event.RequestId,
                    DownloadId = knownRequested.@event.DownloadId,
                    When = knownRequested.@event.When
                };

                var expected = new OverlappingTransactionZonesRecord
                {
                    DownloadId = knownRequested.expected.DownloadId,
                    Description = knownRequested.expected.Description,
                    Contour = knownRequested.expected.Contour
                };

                return new
                {
                    @event,
                    expected
                };
            }).ToList();

        return new OverlappingTransactionZonesProjection()
            .Scenario()
            .Given(requestedData.Select(d => d.@event))
            .Given(changesAcceptedData.Select(d => d.@event))
            .Expect(changesAcceptedData.Select(d => d.expected));
    }
}
