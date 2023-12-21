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
using Polygon = NetTopologySuite.Geometries.Polygon;

public class ExtractRequestOverlapRecordProjectionTests
{
    private readonly Fixture _fixture;

    public ExtractRequestOverlapRecordProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.Customize<RoadNetworkExtractGotRequested>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractGotRequested
                            {
                                Description = _fixture.Create<ExtractDescription>(),
                                DownloadId = _fixture.Create<Guid>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                Contour = new RoadNetworkExtractGeometry
                                {
                                    SpatialReferenceSystemIdentifier =
                                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                    MultiPolygon = Array.Empty<BackOffice.Messages.Polygon>(),
                                    Polygon = null
                                },
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant()),
                                IsInformative = false
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
        _fixture.Customize<RoadNetworkExtractGotRequestedV2>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractGotRequestedV2
                            {
                                Description = _fixture.Create<ExtractDescription>(),
                                DownloadId = _fixture.Create<Guid>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                Contour = new RoadNetworkExtractGeometry
                                {
                                    SpatialReferenceSystemIdentifier =
                                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                    MultiPolygon = Array.Empty<BackOffice.Messages.Polygon>(),
                                    Polygon = null
                                },
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant()),
                                IsInformative = false
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
        _fixture.Customize<RoadNetworkExtractClosed>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractClosed
                            {
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ExternalRequestId = externalRequestId,
                                DownloadIds = _fixture.CreateMany<string>(Random.Shared.Next(1, 5)).ToArray(),
                                DateRequested = DateTime.UtcNow,
                                Reason = RoadNetworkExtractCloseReason.NoDownloadReceived,
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    [Fact]
    public Task When_extract_got_requested_without_overlapping()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequested>();

        return new ExtractRequestOverlapRecordProjection()
            .Scenario()
            .Given(extract1)
            .Expect();
    }

    //NOTE: can only test it through integration testing
    //[Fact]
    //public Task When_extract_got_requested_with_overlapping()
    //{
    //    var contour1 = new Polygon(new LinearRing(new Coordinate[]
    //    {
    //        new(0, 0),
    //        new(0, 10),
    //        new(10, 10),
    //        new(10, 0),
    //        new(0, 0),
    //    }));
    //    var contour2 = new Polygon(new LinearRing(new Coordinate[]
    //    {
    //        new(5, 0),
    //        new(5, 10),
    //        new(15, 10),
    //        new(15, 0),
    //        new(5, 0),
    //    }));
    //    var overlap = new Polygon(new LinearRing(new Coordinate[]
    //    {
    //        new(5, 0),
    //        new(5, 10),
    //        new(10, 10),
    //        new(10, 0),
    //        new(5, 0),
    //    }));

    //    var extract1 = _fixture.Create<RoadNetworkExtractGotRequested>();
    //    extract1.Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour1);
    //    var extract2 = _fixture.Create<RoadNetworkExtractGotRequested>();
    //    extract2.Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour2);

    //    var expected = new ExtractRequestOverlapRecord
    //    {
    //        Id = 1,
    //        DownloadId1 = extract1.DownloadId,
    //        DownloadId2 = extract2.DownloadId,
    //        Description1 = extract1.Description,
    //        Description2 = extract2.Description,
    //        Contour = overlap
    //    };

    //    return new ExtractRequestOverlapRecordProjection()
    //        .Scenario()
    //        .Given(extract1, extract2)
    //        .Expect(expected);
    //}

    //TODO-rik unit tests
    //[Fact]
    //public Task When_extract_closed()
    //{
    //    var requestedData = _fixture
    //        .CreateMany<RoadNetworkExtractGotRequestedV2>()
    //        .Select(requested =>
    //        {
    //            var expected = new ExtractRequestOverlapRecord
    //            {
    //                RequestedOn = DateTime.Parse(requested.When),
    //                ExternalRequestId = requested.ExternalRequestId,
    //                Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
    //                DownloadId = requested.DownloadId,
    //                Description = requested.Description,
    //                IsInformative = requested.IsInformative
    //            };

    //            return new
    //            {
    //                @event = requested,
    //                expected
    //            };
    //        }).ToList();

    //    var closedData = requestedData
    //        .Select(knownRequested =>
    //        {
    //            var @event = new RoadNetworkExtractClosed
    //            {
    //                RequestId = knownRequested.@event.RequestId,
    //                ExternalRequestId = knownRequested.@event.ExternalRequestId,
    //                DownloadIds = new [] { knownRequested.@event.DownloadId.ToString("N") },
    //                DateRequested = DateTime.UtcNow,
    //                Reason = _fixture.Create<RoadNetworkExtractCloseReason>(),
    //                When = knownRequested.@event.When
    //            };

    //            var expected = new ExtractRequestOverlapRecord
    //            {
    //                DownloadId = knownRequested.expected.DownloadId,
    //                ExternalRequestId = knownRequested.expected.ExternalRequestId,
    //                Description = knownRequested.expected.Description,
    //                Contour = knownRequested.expected.Contour,
    //                RequestedOn = knownRequested.expected.RequestedOn,
    //                IsInformative = true
    //            };

    //            return new
    //            {
    //                @event = @event,
    //                expected
    //            };
    //        }).ToList();

    //    return new ExtractRequestOverlapRecordProjection()
    //        .Scenario()
    //        .Given(requestedData.Select(d => d.@event))
    //        .Given(closedData.Select(d => d.@event))
    //        .Expect(closedData.Select(d => d.expected));
    //}

    //[Fact]
    //public Task When_extract_upload_changes_got_accepted()
    //{
    //    var requestedData = _fixture
    //        .CreateMany<RoadNetworkExtractGotRequestedV2>()
    //        .Select(requested =>
    //        {
    //            var expected = new ExtractRequestOverlapRecord
    //            {
    //                RequestedOn = DateTime.Parse(requested.When),
    //                ExternalRequestId = requested.ExternalRequestId,
    //                Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
    //                DownloadId = requested.DownloadId,
    //                Description = requested.Description,
    //                IsInformative = requested.IsInformative
    //            };

    //            return new
    //            {
    //                @event = requested,
    //                expected
    //            };
    //        }).ToList();

    //    var changesAcceptedData = requestedData
    //        .Select(knownRequested =>
    //        {
    //            var @event = new RoadNetworkChangesAccepted
    //            {
    //                RequestId = knownRequested.@event.RequestId,
    //                DownloadId = knownRequested.@event.DownloadId,
    //                When = knownRequested.@event.When
    //            };

    //            var expected = new ExtractRequestOverlapRecord
    //            {
    //                DownloadId = knownRequested.expected.DownloadId,
    //                ExternalRequestId = knownRequested.expected.ExternalRequestId,
    //                Description = knownRequested.expected.Description,
    //                Contour = knownRequested.expected.Contour,
    //                RequestedOn = knownRequested.expected.RequestedOn,
    //                IsInformative = true
    //            };

    //            return new
    //            {
    //                @event = @event,
    //                expected
    //            };
    //        }).ToList();

    //    return new ExtractRequestOverlapRecordProjection()
    //        .Scenario()
    //        .Given(requestedData.Select(d => d.@event))
    //        .Given(changesAcceptedData.Select(d => d.@event))
    //        .Expect(changesAcceptedData.Select(d => d.expected));
    //}
}
