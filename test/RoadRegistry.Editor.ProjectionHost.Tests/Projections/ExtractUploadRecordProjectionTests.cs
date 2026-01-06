namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extracts;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class ExtractUploadRecordProjectionTests
{
    private readonly Fixture _fixture;
    private readonly Instant _instant = SystemClock.Instance.GetCurrentInstant();

    public ExtractUploadRecordProjectionTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.Customize<RoadNetworkExtractChangesArchiveUploaded>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractChangesArchiveUploaded
                            {
                                UploadId = _fixture.Create<UploadId>(),
                                DownloadId = _fixture.Create<DownloadId>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ArchiveId = _fixture.Create<ArchiveId>(),
                                When = InstantPattern.ExtendedIso.Format(_instant)
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    [Fact]
    public Task When_changes_got_accepted()
    {
        var uploadId = _fixture.Create<UploadId>();

        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeTransactionId();
        _fixture.Customize<RoadNetworkChangesAccepted>(
            customization =>
                customization
                    .FromFactory(generator => new RoadNetworkChangesAccepted
                    {
                        RequestId = ChangeRequestId.FromUploadId(uploadId),
                        Reason = _fixture.Create<Reason>(),
                        Operator = _fixture.Create<OperatorName>(),
                        OrganizationId = _fixture.Create<OrganizationId>(),
                        Organization = _fixture.Create<OrganizationName>(),
                        TransactionId = _fixture.Create<TransactionId>(),
                        Changes = Array.Empty<AcceptedChange>(),
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    })
                    .OmitAutoProperties()
        );
        var data = _fixture
            .CreateMany<RoadNetworkChangesAccepted>(1)
            .Select(accepted =>
            {
                var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                var expected = new ExtractUploadRecord
                {
                    UploadId = uploadId,
                    DownloadId = _fixture.Create<DownloadId>(),
                    RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                    ExternalRequestId = externalRequestId,
                    ArchiveId = _fixture.Create<ArchiveId>(),
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(uploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(accepted.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.ChangesAccepted,
                    CompletedOn = InstantPattern.ExtendedIso.Parse(accepted.When).Value.ToUnixTimeSeconds()
                };

                return new
                {
                    given = new RoadNetworkExtractChangesArchiveUploaded
                    {
                        UploadId = expected.UploadId,
                        DownloadId = expected.DownloadId,
                        ExternalRequestId = expected.ExternalRequestId,
                        RequestId = expected.RequestId,
                        ArchiveId = expected.ArchiveId,
                        When = accepted.When
                    },
                    @event = accepted,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_changes_got_accepted_but_were_not_caused_by_an_extract_upload()
    {
        var uploadId = _fixture.Create<UploadId>();

        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeTransactionId();
        _fixture.Customize<RoadNetworkChangesAccepted>(
            customization =>
                customization
                    .FromFactory(generator => new RoadNetworkChangesAccepted
                    {
                        RequestId = ChangeRequestId.FromUploadId(uploadId),
                        Reason = _fixture.Create<Reason>(),
                        Operator = _fixture.Create<OperatorName>(),
                        OrganizationId = _fixture.Create<OrganizationId>(),
                        Organization = _fixture.Create<OrganizationName>(),
                        TransactionId = _fixture.Create<TransactionId>(),
                        Changes = Array.Empty<AcceptedChange>(),
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    })
                    .OmitAutoProperties()
        );
        var events = _fixture
            .CreateMany<RoadNetworkChangesAccepted>(1)
            .ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(events)
            .ExpectNone();
    }

    [Fact]
    public Task When_changes_got_rejected()
    {
        var uploadId = _fixture.Create<UploadId>();

        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeTransactionId();
        _fixture.Customize<RoadNetworkChangesRejected>(
            customization =>
                customization
                    .FromFactory(generator => new RoadNetworkChangesRejected
                    {
                        RequestId = ChangeRequestId.FromUploadId(uploadId),
                        Reason = _fixture.Create<Reason>(),
                        Operator = _fixture.Create<OperatorName>(),
                        OrganizationId = _fixture.Create<OrganizationId>(),
                        Organization = _fixture.Create<OrganizationName>(),
                        TransactionId = _fixture.Create<TransactionId>(),
                        Changes = Array.Empty<RejectedChange>(),
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    })
                    .OmitAutoProperties()
        );
        var data = _fixture
            .CreateMany<RoadNetworkChangesRejected>(1)
            .Select(rejected =>
            {
                var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                var expected = new ExtractUploadRecord
                {
                    UploadId = uploadId,
                    DownloadId = _fixture.Create<DownloadId>(),
                    RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                    ExternalRequestId = externalRequestId,
                    ArchiveId = _fixture.Create<ArchiveId>(),
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(uploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.ChangesRejected,
                    CompletedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds()
                };

                return new
                {
                    given = new RoadNetworkExtractChangesArchiveUploaded
                    {
                        UploadId = expected.UploadId,
                        DownloadId = expected.DownloadId,
                        ExternalRequestId = expected.ExternalRequestId,
                        RequestId = expected.RequestId,
                        ArchiveId = expected.ArchiveId,
                        When = rejected.When
                    },
                    @event = rejected,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_changes_got_rejected_but_were_not_caused_by_an_extract_upload()
    {
        var uploadId = _fixture.Create<UploadId>();

        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeTransactionId();
        _fixture.Customize<RoadNetworkChangesRejected>(
            customization =>
                customization
                    .FromFactory(generator => new RoadNetworkChangesRejected
                    {
                        RequestId = ChangeRequestId.FromUploadId(uploadId),
                        Reason = _fixture.Create<Reason>(),
                        Operator = _fixture.Create<OperatorName>(),
                        OrganizationId = _fixture.Create<OrganizationId>(),
                        Organization = _fixture.Create<OrganizationName>(),
                        TransactionId = _fixture.Create<TransactionId>(),
                        Changes = Array.Empty<RejectedChange>(),
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    })
                    .OmitAutoProperties()
        );

        var events = _fixture
            .CreateMany<RoadNetworkChangesRejected>(1)
            .ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(events)
            .ExpectNone();
    }

    [Fact]
    public Task When_extract_changes_archive_got_accepted()
    {
        _fixture.Customize<RoadNetworkExtractChangesArchiveAccepted>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractChangesArchiveAccepted
                            {
                                UploadId = _fixture.Create<UploadId>(),
                                DownloadId = _fixture.Create<DownloadId>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ArchiveId = _fixture.Create<ArchiveId>(),
                                When = InstantPattern.ExtendedIso.Format(_instant)
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
        var data = _fixture
            .CreateMany<RoadNetworkExtractChangesArchiveAccepted>(1)
            .Select(accepted =>
            {
                var expected = new ExtractUploadRecord
                {
                    UploadId = accepted.UploadId,
                    DownloadId = accepted.DownloadId,
                    RequestId = accepted.RequestId,
                    ExternalRequestId = accepted.ExternalRequestId,
                    ArchiveId = accepted.ArchiveId,
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(accepted.UploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(accepted.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.UploadAccepted,
                    CompletedOn = 0L
                };

                return new
                {
                    given = new RoadNetworkExtractChangesArchiveUploaded
                    {
                        UploadId = accepted.UploadId,
                        DownloadId = accepted.DownloadId,
                        ExternalRequestId = accepted.ExternalRequestId,
                        RequestId = accepted.RequestId,
                        ArchiveId = accepted.ArchiveId,
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    },
                    @event = accepted,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_changes_archive_got_rejected()
    {
        _fixture.Customize<RoadNetworkExtractChangesArchiveRejected>(
            customization =>
                customization
                    .FromFactory(generator =>
                        {
                            var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractChangesArchiveRejected
                            {
                                UploadId = _fixture.Create<UploadId>(),
                                DownloadId = _fixture.Create<DownloadId>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ArchiveId = _fixture.Create<ArchiveId>(),
                                When = InstantPattern.ExtendedIso.Format(_instant)
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
        var data = _fixture
            .CreateMany<RoadNetworkExtractChangesArchiveRejected>(1)
            .Select(rejected =>
            {
                var expected = new ExtractUploadRecord
                {
                    UploadId = rejected.UploadId,
                    DownloadId = rejected.DownloadId,
                    RequestId = rejected.RequestId,
                    ExternalRequestId = rejected.ExternalRequestId,
                    ArchiveId = rejected.ArchiveId,
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(rejected.UploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.UploadRejected,
                    CompletedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds()
                };

                return new
                {
                    given = new RoadNetworkExtractChangesArchiveUploaded
                    {
                        UploadId = rejected.UploadId,
                        DownloadId = rejected.DownloadId,
                        ExternalRequestId = rejected.ExternalRequestId,
                        RequestId = rejected.RequestId,
                        ArchiveId = rejected.ArchiveId,
                        When = rejected.When
                    },
                    @event = rejected,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_extract_changes_got_uploaded()
    {
        var data = _fixture
            .CreateMany<RoadNetworkExtractChangesArchiveUploaded>()
            .Select(uploaded =>
            {
                var expected = new ExtractUploadRecord
                {
                    UploadId = uploaded.UploadId,
                    DownloadId = uploaded.DownloadId,
                    RequestId = uploaded.RequestId,
                    ExternalRequestId = uploaded.ExternalRequestId,
                    ArchiveId = uploaded.ArchiveId,
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(uploaded.UploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(uploaded.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.Received,
                    CompletedOn = 0L
                };

                return new
                {
                    @event = uploaded,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_no_changes()
    {
        var uploadId = _fixture.Create<UploadId>();

        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeTransactionId();
        _fixture.Customize<NoRoadNetworkChanges>(
            customization =>
                customization
                    .FromFactory(generator => new NoRoadNetworkChanges
                    {
                        RequestId = ChangeRequestId.FromUploadId(uploadId),
                        Reason = _fixture.Create<Reason>(),
                        Operator = _fixture.Create<OperatorName>(),
                        OrganizationId = _fixture.Create<OrganizationId>(),
                        Organization = _fixture.Create<OrganizationName>(),
                        TransactionId = _fixture.Create<TransactionId>(),
                        When = InstantPattern.ExtendedIso.Format(_instant)
                    })
                    .OmitAutoProperties()
        );

        var data = _fixture
            .CreateMany<NoRoadNetworkChanges>(1)
            .Select(rejected =>
            {
                var externalRequestId = _fixture.Create<ExternalExtractRequestId>();
                var expected = new ExtractUploadRecord
                {
                    UploadId = uploadId,
                    DownloadId = _fixture.Create<DownloadId>(),
                    RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                    ExternalRequestId = externalRequestId,
                    ArchiveId = _fixture.Create<ArchiveId>(),
                    ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(uploadId)),
                    ReceivedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds(),
                    Status = ExtractUploadStatus.NoChanges,
                    CompletedOn = InstantPattern.ExtendedIso.Parse(rejected.When).Value.ToUnixTimeSeconds()
                };

                return new
                {
                    given = new RoadNetworkExtractChangesArchiveUploaded
                    {
                        UploadId = expected.UploadId,
                        DownloadId = expected.DownloadId,
                        ExternalRequestId = expected.ExternalRequestId,
                        RequestId = expected.RequestId,
                        ArchiveId = expected.ArchiveId,
                        When = rejected.When
                    },
                    @event = rejected,
                    expected
                };
            }).ToList();

        return new ExtractUploadRecordProjection()
            .Scenario()
            .Given(data.SelectMany(d => new object[] { d.given, d.@event }))
            .Expect(data.Select(d => d.expected));
    }
}
