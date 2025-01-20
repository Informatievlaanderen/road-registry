namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema.Extracts;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Polygon = BackOffice.Messages.Polygon;

public class ExtractRequestRecordProjectionTests
{
    private readonly Fixture _fixture;

    public ExtractRequestRecordProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGotRequested();
        _fixture.CustomizeRoadNetworkExtractGotRequestedV2();
        _fixture.CustomizeRoadNetworkExtractClosed();
    }

    [Fact]
    public Task When_extract_got_requested()
    {
        var data = _fixture
            .CreateMany<RoadNetworkExtractGotRequested>()
            .Select(requested =>
            {
                var expected = new ExtractRequestRecord
                {
                    DownloadId = requested.DownloadId,
                    ExternalRequestId = requested.ExternalRequestId,
                    Description = requested.Description,
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    RequestedOn = DateTime.Parse(requested.When),
                    IsInformative = requested.IsInformative
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        return new ExtractRequestRecordProjection()
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
                var expected = new ExtractRequestRecord
                {
                    RequestedOn = DateTime.Parse(requested.When),
                    ExternalRequestId = requested.ExternalRequestId,
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    DownloadId = requested.DownloadId,
                    Description = requested.Description,
                    IsInformative = requested.IsInformative
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        return new ExtractRequestRecordProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_closed()
    {
        var requestedData = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .Select(requested =>
            {
                var expected = new ExtractRequestRecord
                {
                    RequestedOn = DateTime.Parse(requested.When),
                    ExternalRequestId = requested.ExternalRequestId,
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    DownloadId = requested.DownloadId,
                    Description = requested.Description,
                    IsInformative = requested.IsInformative
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        var closedData = requestedData
            .Select(knownRequested =>
            {
                var @event = new RoadNetworkExtractClosed
                {
                    RequestId = knownRequested.@event.RequestId,
                    ExternalRequestId = knownRequested.@event.ExternalRequestId,
                    DownloadIds = new [] { knownRequested.@event.DownloadId.ToString("N") },
                    DateRequested = DateTime.UtcNow,
                    Reason = _fixture.Create<RoadNetworkExtractCloseReason>(),
                    When = knownRequested.@event.When
                };

                var expected = new ExtractRequestRecord
                {
                    DownloadId = knownRequested.expected.DownloadId,
                    ExternalRequestId = knownRequested.expected.ExternalRequestId,
                    Description = knownRequested.expected.Description,
                    Contour = knownRequested.expected.Contour,
                    RequestedOn = knownRequested.expected.RequestedOn,
                    IsInformative = true
                };

                return new
                {
                    @event,
                    expected
                };
            }).ToList();

        return new ExtractRequestRecordProjection()
            .Scenario()
            .Given(requestedData.Select(d => d.@event))
            .Given(closedData.Select(d => d.@event))
            .Expect(closedData.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_upload_changes_got_accepted()
    {
        var requestedData = _fixture
            .CreateMany<RoadNetworkExtractGotRequestedV2>()
            .Select(requested =>
            {
                var expected = new ExtractRequestRecord
                {
                    RequestedOn = DateTime.Parse(requested.When),
                    ExternalRequestId = requested.ExternalRequestId,
                    Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
                    DownloadId = requested.DownloadId,
                    Description = requested.Description,
                    IsInformative = requested.IsInformative
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
                var @event = new RoadNetworkChangesAccepted
                {
                    RequestId = knownRequested.@event.RequestId,
                    DownloadId = knownRequested.@event.DownloadId,
                    When = knownRequested.@event.When
                };

                var expected = new ExtractRequestRecord
                {
                    DownloadId = knownRequested.expected.DownloadId,
                    ExternalRequestId = knownRequested.expected.ExternalRequestId,
                    Description = knownRequested.expected.Description,
                    Contour = knownRequested.expected.Contour,
                    RequestedOn = knownRequested.expected.RequestedOn,
                    IsInformative = true
                };

                return new
                {
                    @event,
                    expected
                };
            }).ToList();

        return new ExtractRequestRecordProjection()
            .Scenario()
            .Given(requestedData.Select(d => d.@event))
            .Given(changesAcceptedData.Select(d => d.@event))
            .Expect(changesAcceptedData.Select(d => d.expected));
    }
}
