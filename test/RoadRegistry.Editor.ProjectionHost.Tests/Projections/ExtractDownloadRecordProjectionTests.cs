namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema.Extracts;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Polygon = BackOffice.Messages.Polygon;

public class ExtractDownloadRecordProjectionTests
{
    private readonly Fixture _fixture;

    public ExtractDownloadRecordProjectionTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGotRequested();
        _fixture.CustomizeRoadNetworkExtractGotRequestedV2();
        _fixture.Customize<RoadNetworkExtractDownloadBecameAvailable>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractDownloadBecameAvailable
                            {
                                Description = _fixture.Create<ExtractDescription>(),
                                DownloadId = _fixture.Create<Guid>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ArchiveId = _fixture.Create<ArchiveId>(),
                                ZipArchiveWriterVersion = _fixture.Create<string>(),
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
        _fixture.Customize<RoadNetworkExtractDownloadTimeoutOccurred>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractDownloadTimeoutOccurred
                            {
                                Description = _fixture.Create<ExtractDescription>(),
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ExternalRequestId = externalRequestId,
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    [Fact]
    public Task When_extract_download_became_available()
    {
        var data = _fixture
            .CreateMany<RoadNetworkExtractDownloadBecameAvailable>()
            .Select(available =>
            {
                var expected = new ExtractDownloadRecord
                {
                    DownloadId = available.DownloadId,
                    RequestId = available.RequestId,
                    ExternalRequestId = available.ExternalRequestId,
                    ArchiveId = available.ArchiveId,
                    Available = true,
                    ZipArchiveWriterVersion = available.ZipArchiveWriterVersion,
                    AvailableOn = InstantPattern.ExtendedIso.Parse(available.When).Value.ToUnixTimeSeconds(),
                    RequestedOn = InstantPattern.ExtendedIso.Parse(available.When).Value.ToUnixTimeSeconds()
                };

                return new
                {
                    given = new RoadNetworkExtractGotRequested
                    {
                        Description = _fixture.Create<ExtractDescription>(),
                        ExternalRequestId = available.ExternalRequestId,
                        RequestId = available.RequestId,
                        DownloadId = available.DownloadId,
                        Contour = new RoadNetworkExtractGeometry
                        {
                            SpatialReferenceSystemIdentifier =
                                SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                            MultiPolygon = Array.Empty<Polygon>(),
                            Polygon = null
                        },
                        When = InstantPattern.ExtendedIso.Format(InstantPattern.ExtendedIso.Parse(available.When).Value)
                    },
                    @event = available,
                    expected
                };
            }).ToList();

        return new ExtractDownloadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public async Task When_extract_download_timeout_occurred()
    {
        var gotRequested = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        var timeoutOccurred = _fixture.Create<RoadNetworkExtractDownloadTimeoutOccurred>();
        timeoutOccurred.DownloadId = gotRequested.DownloadId;
        timeoutOccurred.ExternalRequestId = gotRequested.ExternalRequestId;
        timeoutOccurred.RequestId = gotRequested.RequestId;

        await new ExtractDownloadRecordProjection()
            .Scenario()
            .Given(gotRequested)
            .Given(timeoutOccurred)
            .Expect(new ExtractDownloadRecord
            {
                DownloadId = timeoutOccurred.DownloadId.Value,
                RequestId = timeoutOccurred.RequestId,
                ExternalRequestId = timeoutOccurred.ExternalRequestId,
                ArchiveId = null,
                Available = true,
                AvailableOn = InstantPattern.ExtendedIso.Parse(timeoutOccurred.When).Value.ToUnixTimeSeconds(),
                RequestedOn = InstantPattern.ExtendedIso.Parse(gotRequested.When).Value.ToUnixTimeSeconds()
            });
    }

    [Fact]
    public async Task When_extract_download_timeout_occurred_with_multiple_extract_downloads()
    {
        var download1GotRequested = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        var download1BecameAvailable = _fixture.Create<RoadNetworkExtractDownloadBecameAvailable>();
        download1BecameAvailable.DownloadId = download1GotRequested.DownloadId;
        download1BecameAvailable.ExternalRequestId = download1GotRequested.ExternalRequestId;
        download1BecameAvailable.RequestId = download1GotRequested.RequestId;

        var download2GotRequested = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        download2GotRequested.ExternalRequestId = download1GotRequested.ExternalRequestId;
        download2GotRequested.RequestId = download1GotRequested.RequestId;

        var download2TimeoutOccurred = _fixture.Create<RoadNetworkExtractDownloadTimeoutOccurred>();
        download2TimeoutOccurred.DownloadId = download2GotRequested.DownloadId;
        download2TimeoutOccurred.ExternalRequestId = download2GotRequested.ExternalRequestId;
        download2TimeoutOccurred.RequestId = download2GotRequested.RequestId;

        await new ExtractDownloadRecordProjection()
            .Scenario()
            .Given(download1GotRequested)
            .Given(download1BecameAvailable)
            .Given(download2GotRequested)
            .Given(download2TimeoutOccurred)
            .Expect(
                new ExtractDownloadRecord
                {
                    DownloadId = download1GotRequested.DownloadId,
                    RequestId = download1GotRequested.RequestId,
                    ExternalRequestId = download1GotRequested.ExternalRequestId,
                    ArchiveId = download1BecameAvailable.ArchiveId,
                    Available = true,
                    AvailableOn = InstantPattern.ExtendedIso.Parse(download1BecameAvailable.When).Value.ToUnixTimeSeconds(),
                    RequestedOn = InstantPattern.ExtendedIso.Parse(download1GotRequested.When).Value.ToUnixTimeSeconds(),
                    ZipArchiveWriterVersion = download1BecameAvailable.ZipArchiveWriterVersion
                },
                new ExtractDownloadRecord
                {
                    DownloadId = download2GotRequested.DownloadId,
                    RequestId = download2GotRequested.RequestId,
                    ExternalRequestId = download2GotRequested.ExternalRequestId,
                    ArchiveId = null,
                    Available = true,
                    AvailableOn = InstantPattern.ExtendedIso.Parse(download2TimeoutOccurred.When).Value.ToUnixTimeSeconds(),
                    RequestedOn = InstantPattern.ExtendedIso.Parse(download2GotRequested.When).Value.ToUnixTimeSeconds()
                });
    }

    [Fact]
    public Task When_extract_got_requested()
    {
        var data = _fixture
            .CreateMany<RoadNetworkExtractGotRequested>()
            .Select(requested =>
            {
                var expected = new ExtractDownloadRecord
                {
                    DownloadId = requested.DownloadId,
                    RequestId = requested.RequestId,
                    ExternalRequestId = requested.ExternalRequestId,
                    ArchiveId = null,
                    Available = false,
                    AvailableOn = 0L,
                    RequestedOn = InstantPattern.ExtendedIso.Parse(requested.When).Value.ToUnixTimeSeconds(),
                    IsInformative = false
                };

                return new
                {
                    @event = requested,
                    expected
                };
            }).ToList();

        return new ExtractDownloadRecordProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }
}
