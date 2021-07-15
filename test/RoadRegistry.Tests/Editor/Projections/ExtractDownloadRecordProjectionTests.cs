namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Framework.Projections;
    using NodaTime;
    using NodaTime.Text;
    using Schema.Extracts;
    using Xunit;

    public class ExtractDownloadRecordProjectionTests
    {
        private readonly Fixture _fixture;

        public ExtractDownloadRecordProjectionTests()
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
                                    DownloadId = _fixture.Create<Guid>(),
                                    ExternalRequestId = externalRequestId,
                                    RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                    Contour = new RoadNetworkExtractGeometry
                                    {
                                        SpatialReferenceSystemIdentifier =
                                            SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                        MultiPolygon = new BackOffice.Messages.Polygon[0]
                                    },
                                    When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                                };
                            }
                        )
                        .OmitAutoProperties()
            );
            _fixture.Customize<RoadNetworkExtractDownloadBecameAvailable>(
                customization =>
                    customization
                        .FromFactory(generator =>
                            {
                                var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                                return new RoadNetworkExtractDownloadBecameAvailable
                                {
                                    DownloadId = _fixture.Create<Guid>(),
                                    ExternalRequestId = externalRequestId,
                                    RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                    ArchiveId = _fixture.Create<ArchiveId>(),
                                    When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                                };
                            }
                        )
                        .OmitAutoProperties()
            );
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
                        RequestedOn = InstantPattern.ExtendedIso.Parse(requested.When).Value.ToUnixTimeTicks()
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
                        AvailableOn = InstantPattern.ExtendedIso.Parse(available.When).Value.ToUnixTimeTicks(),
                        RequestedOn = InstantPattern.ExtendedIso.Parse(available.When).Value.ToUnixTimeTicks()
                    };

                    return new
                    {
                        given = new RoadNetworkExtractGotRequested
                        {
                            ExternalRequestId = available.ExternalRequestId,
                            RequestId = available.RequestId,
                            DownloadId = available.DownloadId,
                            Contour = new RoadNetworkExtractGeometry
                            {
                                SpatialReferenceSystemIdentifier =
                                    SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                MultiPolygon = new BackOffice.Messages.Polygon[0]
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
    }
}
